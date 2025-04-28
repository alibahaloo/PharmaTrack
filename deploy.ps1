<#
.SYNOPSIS
    Publishes multiple .NET API projects and a WPF app, generates a shared self-signed cert,
    installs it into the machine store (and trust root), deploys each API as a Windows Service,
    and creates a desktop shortcut for the WPF app.

.DESCRIPTION
    • Generates a single self-signed cert (with SAN for localhost) and exports it to .\certs\PharmaTrackCert.pfx  
    • Imports that PFX into LocalMachine\My and into LocalMachine\Root (trusted CA)  
    • Grants the LocalSystem account read-access to the cert’s private key  
    • Publishes each API project, copies the PFX into its publish folder, installs/starts as a Windows Service  
    • Publishes the WPF project and creates a desktop shortcut  
    • Ensures .NET 9 SDK and LocalDB instance are installed and running  
    • Must be run as Administrator
#>

# Stop on any error
$ErrorActionPreference = 'Stop'

#region Configuration

# List of API projects to publish & install
$projects = @(
    @{ ProjectPath = ".\Auth.API\Auth.API.csproj";        PublishDir = ".\publish\AuthAPI";      ServiceName = "PharmaTrackAuthAPI";      DisplayName = "PharmaTrack Auth API Service";        Description = "PharmaTrack Auth.API as Windows Service" },
    @{ ProjectPath = ".\Schedule.API\Schedule.API.csproj"; PublishDir = ".\publish\ScheduleAPI"; ServiceName = "PharmaTrackScheduleAPI"; DisplayName = "PharmaTrack Schedule API Service";  Description = "PharmaTrack Schedule.API as Windows Service" },
    @{ ProjectPath = ".\Gateway.API\Gateway.API.csproj";   PublishDir = ".\publish\GatewayAPI";  ServiceName = "PharmaTrackGatewayAPI";  DisplayName = "PharmaTrack Gateway API Service";   Description = "PharmaTrack Gateway.API as Windows Service" },
    @{ ProjectPath = ".\Drug.API\Drug.API.csproj";         PublishDir = ".\publish\DrugAPI";     ServiceName = "PharmaTrackDrugAPI";      DisplayName = "PharmaTrack Drug API Service";      Description = "PharmaTrack Drug.API as Windows Service" },
    @{ ProjectPath = ".\Inventory.API\Inventory.API.csproj";PublishDir = ".\publish\InventoryAPI"; ServiceName = "PharmaTrackInventoryAPI"; DisplayName = "PharmaTrack Inventory API Service"; Description = "PharmaTrack Inventory.API as Windows Service" }
)

# Certificate settings
$certSubject    = 'CN=PharmaTrack'
$certValidYears = 1
$pfxPassword    = 'YourP@ssw0rd!'

# WPF project path
$wpfProjectPath = ".\PharmaTrack.WPF\PharmaTrack.WPF.csproj"

# Script directories
$scriptDir      = Split-Path -Parent $MyInvocation.MyCommand.Definition
$centralCertDir = Join-Path $scriptDir 'certs'
$centralPfxPath = Join-Path $centralCertDir 'PharmaTrackCert.pfx'

#endregion

#region Helpers

function Assert-Admin {
    $identity  = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        Write-Host "ERROR: This script must be run as Administrator. Exiting now." -ForegroundColor Red
        exit 1
    }
}

function Ensure-DotNet9 {
    Write-Host "INFO: Checking for .NET 9 SDK..." -ForegroundColor Cyan
    if (-not (dotnet --list-sdks | Select-String '^9\.')) {
        Write-Host "WARNING: .NET 9 SDK not found. Installing silently..." -ForegroundColor Yellow
        winget install --id Microsoft.DotNet.SDK.9 -e --silent
        Write-Host "SUCCESS: .NET 9 SDK installed." -ForegroundColor Green
    } else {
        Write-Host "SUCCESS: .NET 9 SDK is already installed." -ForegroundColor Green
    }
}

function Remove-ServiceIfExists {
    param([string]$name)
    if (Get-Service -Name $name -ErrorAction SilentlyContinue) {
        Write-Host "WARNING: Stopping & deleting existing service '$name'..." -ForegroundColor Yellow
        Stop-Service -Name $name -Force -ErrorAction SilentlyContinue
        sc.exe delete $name | Out-Null
        Start-Sleep -Seconds 2
    }
}

function Install-Service {
    param(
        [string]$name,
        [string]$display,
        [string]$binPath,
        [string]$desc
    )
    Write-Host "INFO: Installing service '$name'..." -ForegroundColor Cyan
    New-Service -Name $name -BinaryPathName "`"$binPath`"" -DisplayName $display -StartupType Automatic
    Set-Service   -Name $name -Description $desc
    Start-Service -Name $name
    Write-Host "SUCCESS: Service '$name' is running." -ForegroundColor Green
}

#endregion

#region Deploy Functions

function Deploy-Certificates {
    Write-Host "`nINFO: Deploying certificates..." -ForegroundColor Cyan
    New-Item -ItemType Directory -Path $centralCertDir -Force | Out-Null
    if (-not (Test-Path $centralPfxPath)) {
        $cert      = New-SelfSignedCertificate -Subject $certSubject -DnsName "localhost" -CertStoreLocation 'Cert:\LocalMachine\My' -NotAfter (Get-Date).AddYears($certValidYears)
        $securePwd = ConvertTo-SecureString -String $pfxPassword -AsPlainText -Force
        Export-PfxCertificate -Cert $cert -FilePath $centralPfxPath -Password $securePwd

        Import-PfxCertificate -FilePath $centralPfxPath -CertStoreLocation Cert:\LocalMachine\My -Password $securePwd
        Import-PfxCertificate -FilePath $centralPfxPath -CertStoreLocation Cert:\LocalMachine\Root -Password $securePwd

        $thumb       = $cert.Thumbprint.Replace(' ', '').ToUpper()
        $machineCert = Get-ChildItem Cert:\LocalMachine\My | Where-Object Thumbprint -eq $thumb
        $keyName     = $machineCert.PrivateKey.CspKeyContainerInfo.UniqueKeyContainerName
        $keyPath     = Join-Path $env:ProgramData "Microsoft\Crypto\RSA\MachineKeys\$keyName"

        Write-Host "INFO: Granting LocalSystem read-access to the private key..." -ForegroundColor Cyan
        icacls $keyPath /grant "NT AUTHORITY\SYSTEM:(R)" | Out-Null
        Write-Host "SUCCESS: Certificate created, trusted, and permissions set." -ForegroundColor Green
    } else {
        Write-Host "INFO: PFX exists at $centralPfxPath – skipping." -ForegroundColor Cyan
    }
}

function Deploy-APIs {
    Write-Host "`nINFO: Deploying APIs..." -ForegroundColor Cyan

    foreach ($proj in $projects) { Remove-ServiceIfExists -name $proj.ServiceName }

    foreach ($proj in $projects) {
        $outDir = Join-Path $scriptDir $proj.PublishDir
        Write-Host "INFO: Publishing $($proj.ProjectPath) to $outDir..." -ForegroundColor Cyan
        dotnet publish $proj.ProjectPath -c Release -r win-x64 --self-contained true -o $outDir

        Write-Host "INFO: Copying cert to $outDir\certs" -ForegroundColor Cyan
        New-Item -ItemType Directory -Path (Join-Path $outDir 'certs') -Force | Out-Null
        Copy-Item -Path $centralPfxPath -Destination (Join-Path $outDir 'certs') -Force

        $exePath = Join-Path $outDir ([IO.Path]::GetFileNameWithoutExtension($proj.ProjectPath) + '.exe')
        Remove-ServiceIfExists -name $proj.ServiceName
        Install-Service        -name $proj.ServiceName -display $proj.DisplayName -binPath $exePath -desc $proj.Description
    }
}

function Deploy-WPF {
    Write-Host "`nINFO: Deploying WPF app and creating shortcut..." -ForegroundColor Cyan
    $wpfOut    = Join-Path $scriptDir 'publish\PharmaTrack.WPF'
    dotnet publish $wpfProjectPath -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $wpfOut

    $targetExe = Join-Path $wpfOut 'PharmaTrack.WPF.exe'
    $desktop   = [Environment]::GetFolderPath('Desktop')
    $linkPath  = Join-Path $desktop 'PharmaTrack.lnk'
    $wshell    = New-Object -ComObject WScript.Shell
    $shortcut  = $wshell.CreateShortcut($linkPath)

    $shortcut.TargetPath       = $targetExe
    $shortcut.WorkingDirectory = Split-Path $targetExe
    $shortcut.WindowStyle      = 1
    $shortcut.Description      = 'PharmaTrack WPF App'
    $shortcut.Save()

    Write-Host "SUCCESS: WPF app deployed and shortcut created at $linkPath" -ForegroundColor Green
}

#endregion

#region Import Data Functions

function Import-Initial-Data {
    Write-Host "`nINFO: Importing initial data into the system..." -ForegroundColor Cyan
    Write-Host "`INFO: Waiting for API readiness via /health..." -ForegroundColor Cyan
    $healthUrl   = 'https://localhost:8086/health'
    $maxRetries  = 12          # e.g. 12 attempts
    $delaySecs   = 5           # 5 seconds between tries
    $attempt     = 0

    do {
        $attempt++
        try {
            $resp = Invoke-WebRequest -Uri $healthUrl -Method GET -TimeoutSec 5 -ErrorAction Stop
            if ($resp.StatusCode -eq 200) {
                Write-Host "`nSUCCESS: API is healthy (after $attempt attempt(s))." -ForegroundColor Green
                break
            }
        } catch {
            Write-Host -NoNewline "."
            Start-Sleep -Seconds $delaySecs
        }
        if ($attempt -ge $maxRetries) {
            Write-Host "ERROR: API health check failed: $healthUrl did not return 200 after $maxRetries attempts."; -ForegroundColor Red
            exit 1
        }
    } while ($true)

    # Now that /health is green, do your imports once
    Write-Host "`nINFO: Importing initial data via API endpoints..." -ForegroundColor Cyan

    # POST to import drug data
    try {
        $response = Invoke-WebRequest -Uri "https://localhost:8086/api/Jobs/import-drug-data" -Method POST
    } catch {
        throw "ERROR: Failed to send request to import drug data: $($_.Exception.Message)";
    }
    if ($response.StatusCode -ne 200) {
        Write-Host "ERROR: Import drug data failed with HTTP status code $($response.StatusCode). Stopping Installation." -ForegroundColor Red
        exit 1
    }
    Write-Host "SUCCESS: Drug data import scheduled successfully (Status $($response.StatusCode))." -ForegroundColor Green

    # POST to import interaction data
    try {
        $response = Invoke-WebRequest -Uri "https://localhost:8086/api/Jobs/import-interaction-data" -Method POST
    } catch {
        throw "ERROR: Failed to send request to import interaction data: $($_.Exception.Message)";
    }
    if ($response.StatusCode -ne 200) {
        Write-Host "ERROR: Import interaction data failed with HTTP status code $($response.StatusCode). Stopping Installation." -ForegroundColor Red
        exit 1
    }
    Write-Host "SUCCESS: Interaction data import scheduled successfully (Status $($response.StatusCode))." -ForegroundColor Green

    Write-Host "INFO: You can view the jobs status here: https://localhost:8086/hangfire" -ForegroundColor Cyan
}

function Run-Database-Migrations {
    # Define the list of project files
    $projects = @(
        ".\Auth.API\Auth.API.csproj",
        ".\Schedule.API\Schedule.API.csproj",
        ".\Drug.API\Drug.API.csproj",
        ".\Inventory.API\Inventory.API.csproj"
    )

    # Loop through each project
    foreach ($project in $projects) {
        Write-Host "INFO: Running migrations for $project..." -ForegroundColor Cyan

        # Get the directory of the project
        $projectDir = Split-Path -Path $project -Parent

        # Move to the project directory
        Push-Location $projectDir

        try {
            # Run the migration
            dotnet ef database update
        }
        catch {
            Write-Host "ERROR: Failed to apply migrations for $project" -ForegroundColor Red
            Write-Host $_.Exception.Message
            exit 1
        }
        finally {
            # Always return to the previous directory
            Pop-Location
        }
    }

    Write-Host "SUCCESS: Database migrations completed." -ForegroundColor Green

}

function Ensure-SqlExpressInstallation {
    [CmdletBinding()]
    param(
        [string]$InstanceName = 'SQLEXPRESS'
    )

    $svcName = "MSSQL`$$InstanceName"

    Write-Host "INFO: Checking for SQL Server Express service '$svcName'..." -ForegroundColor Cyan
    $svc = Get-Service -Name $svcName -ErrorAction SilentlyContinue
    if (-not $svc) {
        Write-Host "WARNING: SQL Server Express not found. Installing silently..." -ForegroundColor Yellow
        winget install --id=Microsoft.SQLServer.2022.Express -e | Out-Null
        Write-Host "SUCCESS: SQL Server Express installed." -ForegroundColor Green
        $svc = Get-Service -Name $svcName -ErrorAction Stop
    }

    if ($svc.Status -ne 'Running') {
        Write-Host "INFO: Starting SQL Server Express service..." -ForegroundColor Cyan
        Start-Service -Name $svcName
        $svc.WaitForStatus('Running', (New-TimeSpan -Seconds 30))
    }
    Write-Host "SUCCESS: SQL Server Express service is running." -ForegroundColor Green

    # Ensure NuGet provider
    if (-not (Get-PackageProvider -Name NuGet -ErrorAction SilentlyContinue)) {
        Write-Host "INFO: Installing NuGet provider..." -ForegroundColor Cyan
        Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force | Out-Null
    }
    Import-Module PowerShellGet -Force

    # Ensure SqlServer module
    if (-not (Get-Module -ListAvailable -Name SqlServer)) {
        Write-Host "INFO: Installing SqlServer PowerShell module..." -ForegroundColor Cyan
        Install-Module -Name SqlServer -Scope CurrentUser -Force -AllowClobber | Out-Null
    }
    Import-Module SqlServer -ErrorAction Stop

    # Load SMO for server management
    $mod = Get-Module SqlServer -ListAvailable | Where-Object Name -EQ 'SqlServer' | Select-Object -First 1
    $smoPath = Join-Path $mod.ModuleBase 'bin\Microsoft.SqlServer.Smo.dll'
    if (Test-Path $smoPath) {
        Add-Type -Path $smoPath -ErrorAction Stop
    } else {
        Write-Warning "SMO not found at $smoPath, using fallback load..."
        [System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.Smo') | Out-Null
    }

    # Enable Mixed-Mode authentication
    Write-Host "INFO: Checking/Enabling Mixed-Mode authentication..." -ForegroundColor Cyan
    $server = New-Object Microsoft.SqlServer.Management.Smo.Server "localhost\$InstanceName"
    if ($server.Settings.LoginMode -ne [Microsoft.SqlServer.Management.Smo.ServerLoginMode]::Mixed) {
        $server.Settings.LoginMode = [Microsoft.SqlServer.Management.Smo.ServerLoginMode]::Mixed
        $server.Alter()
        Write-Host "SUCCESS: Mixed-Mode enabled; restarting service..." -ForegroundColor Green
        Restart-Service -Name $svcName -Force
        $server.ConnectionContext.Connect()
    } else {
        Write-Host "SUCCESS: Mixed-Mode already enabled." -ForegroundColor Green
    }
}

# Function 2: Recreate SQL login, force logout, grant full server admin rights, and ensure access on all databases
function Ensure-SqlExpressServerUser {
    [CmdletBinding()]
    param(
        [string]$InstanceName = 'SQLEXPRESS'
    )

    # Hardcoded credentials
    $SqlUser     = 'appuser'
    $SqlPassword = 'P@ssw0rd!'
    # Determine current Windows user
    $WinUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name

    # Connection string to master DB for server-wide tasks
    $connStr = "Server=localhost\$InstanceName;Database=master;Integrated Security=True;TrustServerCertificate=True;"

    # 1) Transfer database ownership for any DBs owned by $SqlUser
    Write-Host "INFO: Transferring ownership of databases from '$SqlUser' to '$WinUser'..." -ForegroundColor Cyan
    $transferTsql = @"
DECLARE @db sysname;
DECLARE db_cursor CURSOR FOR
    SELECT name FROM sys.databases
    WHERE owner_sid = SUSER_SID(N'$SqlUser') AND database_id > 4;
OPEN db_cursor;
FETCH NEXT FROM db_cursor INTO @db;
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('ALTER AUTHORIZATION ON DATABASE::[' + @db + '] TO [$WinUser]');
    FETCH NEXT FROM db_cursor INTO @db;
END;
CLOSE db_cursor;
DEALLOCATE db_cursor;
"@
    Invoke-Sqlcmd -ConnectionString $connStr -Query $transferTsql
    Write-Host "SUCCESS: Ownership transferred." -ForegroundColor Green

    # 2) Force logout and drop/recreate server login with sysadmin rights
    Write-Host "INFO: Forcing logout and recreating login '$SqlUser' with sysadmin rights..." -ForegroundColor Cyan
    $dropLoginTsql = @"
-- Kill all sessions for the login
DECLARE @spid INT;
DECLARE kill_cursor CURSOR FOR
    SELECT session_id FROM sys.dm_exec_sessions WHERE login_name = N'$SqlUser';
OPEN kill_cursor;
FETCH NEXT FROM kill_cursor INTO @spid;
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('KILL ' + @spid);
    FETCH NEXT FROM kill_cursor INTO @spid;
END;
CLOSE kill_cursor;
DEALLOCATE kill_cursor;

-- Drop and recreate login
IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$SqlUser')
BEGIN
    ALTER SERVER ROLE [sysadmin] DROP MEMBER [$SqlUser];
    DROP LOGIN [$SqlUser];
END
CREATE LOGIN [$SqlUser] WITH PASSWORD = N'$SqlPassword', CHECK_POLICY = OFF;
ALTER SERVER ROLE [sysadmin] ADD MEMBER [$SqlUser];
"@
    Invoke-Sqlcmd -ConnectionString $connStr -Query $dropLoginTsql
    Write-Host "SUCCESS: Login '$SqlUser' recreated with sysadmin rights." -ForegroundColor Green

    # 3) Drop and recreate user in each database
    Write-Host "INFO: Dropping and recreating user '$SqlUser' in all databases..." -ForegroundColor Cyan
    $dbUserTsql = @"
DECLARE @name sysname;
DECLARE db_cursor CURSOR FOR
    SELECT name FROM sys.databases
    WHERE database_id > 4 AND state = 0;
OPEN db_cursor;
FETCH NEXT FROM db_cursor INTO @name;
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Force logout from each database
    DECLARE @killspid INT;
    DECLARE user_cursor CURSOR FOR
        SELECT session_id FROM sys.dm_exec_sessions WHERE login_name = N'$SqlUser' AND database_id = DB_ID(@name);
    OPEN user_cursor;
    FETCH NEXT FROM user_cursor INTO @killspid;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC('KILL ' + @killspid);
        FETCH NEXT FROM user_cursor INTO @killspid;
    END;
    CLOSE user_cursor;
    DEALLOCATE user_cursor;

    -- Recreate user mapping
    DECLARE @sql nvarchar(max) = N'
        USE [' + @name + N'];
        IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N''$SqlUser'')
        BEGIN
            DROP USER [$SqlUser];
        END
        CREATE USER [$SqlUser] FOR LOGIN [$SqlUser];
        ALTER ROLE db_owner ADD MEMBER [$SqlUser];
    ';
    EXEC sp_executesql @sql;
    FETCH NEXT FROM db_cursor INTO @name;
END;
CLOSE db_cursor;
DEALLOCATE db_cursor;
"@
    Invoke-Sqlcmd -ConnectionString $connStr -Query $dbUserTsql
    Write-Host "SUCCESS: User '$SqlUser' recreated with db_owner on all databases." -ForegroundColor Green

    Write-Host "`nAll done! '$SqlUser' has full admin rights and ownership transferred to '$WinUser' where needed." -ForegroundColor Magenta
}

#endregion

# ---------- Script Execution ----------
Assert-Admin
Ensure-DotNet9
Ensure-SqlExpressInstallation
Ensure-SqlExpressServerUser

Run-Database-Migrations

Deploy-Certificates
Deploy-APIs

Deploy-WPF
Import-Initial-Data
