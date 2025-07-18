﻿<#
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
    @{
        ProjectPath = ".\PharmaTrack.Host\PharmaTrack.Host.csproj"; 
        PublishDir = ".\publish\Host";
        ServiceName = "PharmaTrackHost";
        DisplayName = "PharmaTrack Host Service";
        Description = "PharmaTrack Host as Windows Service"
    },
    @{
        ProjectPath = ".\Auth.API\Auth.API.csproj";
        PublishDir = ".\publish\AuthAPI";
        ServiceName = "PharmaTrackAuthAPI";
        DisplayName = "PharmaTrack Auth API Service";
        Description = "PharmaTrack Auth.API as Windows Service" 
    },
    @{ 
        ProjectPath = ".\Schedule.API\Schedule.API.csproj";
        PublishDir = ".\publish\ScheduleAPI"; 
        ServiceName = "PharmaTrackScheduleAPI"; 
        DisplayName = "PharmaTrack Schedule API Service";
        Description = "PharmaTrack Schedule.API as Windows Service"
    },
    @{
        ProjectPath = ".\Drug.API\Drug.API.csproj";
        PublishDir = ".\publish\DrugAPI";
        ServiceName = "PharmaTrackDrugAPI";
        DisplayName = "PharmaTrack Drug API Service";
        Description = "PharmaTrack Drug.API as Windows Service"
    },
    @{
        ProjectPath = ".\Inventory.API\Inventory.API.csproj";
        PublishDir = ".\publish\InventoryAPI";
        ServiceName = "PharmaTrackInventoryAPI";
        DisplayName = "PharmaTrack Inventory API Service";
        Description = "PharmaTrack Inventory.API as Windows Service"
    }
)

# Define base URL
$drugApiBaseUrl = "http://localhost:9092"
$authApiBaseUrl = "http://localhost:9091"

# WPF project path
$wpfProjectPath = ".\PharmaTrack.WPF\PharmaTrack.WPF.csproj"

# Script directories
$scriptDir      = Split-Path -Parent $MyInvocation.MyCommand.Definition

$adminUsername = "user@email.com"
$adminPassword = "B4guy#kSDvKJJP+"

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

function Ensure-DotNetEF {
    Write-Host "INFO: Checking for dotnet EF..." -ForegroundColor Cyan

    try {
        dotnet nuget add source https://api.nuget.org/v3/index.json --name nuget.org | Out-Null
        dotnet tool install --global dotnet-ef --ignore-failed-sources --no-cache
        Write-Host "SUCCESS: dotnet-ef installed successfully." -ForegroundColor Green
    }
    catch {
        Write-Host "INFO: dotnet-ef might already be installed. Trying to update it..." -ForegroundColor Cyan
        try {
            dotnet tool update --global dotnet-ef
            Write-Host "SUCCESS: dotnet-ef updated successfully." -ForegroundColor Green
        }
        catch {
            Write-Error "ERROR: Failed to install or update dotnet-ef: $($_.Exception.Message)" -ForegroundColor Red
            exit 1
        }
    }

    Write-Host "INFO: Refreshing PATH for current session..." -ForegroundColor Cyan

    # Add .dotnet\tools to PATH if not already present
    $toolsPath = "$env:USERPROFILE\.dotnet\tools"
    if ($env:Path -notlike "*$toolsPath*") {
        $env:Path += ";$toolsPath"
        Write-Host "SUCCESS: PATH refreshed. You can now use 'dotnet ef' immediately." -ForegroundColor Green
    }
    else {
        Write-Host "INFO: PATH already includes .dotnet\tools. No update needed." -ForegroundColor Cyan
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

function Deploy-APIs {
    Write-Host "`nINFO: Deploying APIs..." -ForegroundColor Cyan

    foreach ($proj in $projects) {
        Remove-ServiceIfExists -name $proj.ServiceName
    }

    foreach ($proj in $projects) {
        $outDir = Join-Path $scriptDir $proj.PublishDir
        Write-Host "INFO: Publishing $($proj.ProjectPath) to $outDir..." -ForegroundColor Cyan
        dotnet publish $proj.ProjectPath -c Release -r win-x64 --self-contained true -o $outDir

        $exePath = Join-Path $outDir ([IO.Path]::GetFileNameWithoutExtension($proj.ProjectPath) + '.exe')
        Remove-ServiceIfExists -name $proj.ServiceName
        Install-Service -name $proj.ServiceName -display $proj.DisplayName -binPath $exePath -desc $proj.Description
    }
}

function Install-Service {
    param(
        [string]$name,
        [string]$display,
        [string]$binPath,   # <-- fully quoted exe + args
        [string]$desc
    )
    Write-Host "INFO: Installing service '$name'..." -ForegroundColor Cyan

    # Create the service using exactly what we passed in:
    New-Service `
      -Name          $name `
      -BinaryPathName $binPath `
      -DisplayName   $display `
      -StartupType   Automatic

    # Add description and start
    Set-Service -Name $name -Description $desc
    Start-Service -Name $name

    Write-Host "SUCCESS: Service '$name' is running." -ForegroundColor Green
}

function Deploy-PWA {
    Write-Host "`nINFO: Deploying PharmaTrack.PWA to its own publish folder…" -ForegroundColor Cyan

    #–– CONFIG ––
    $solutionRoot = $PSScriptRoot
    $pwaProj      = Join-Path $solutionRoot "PharmaTrack.PWA\PharmaTrack.PWA.csproj"
    $pwaPublish   = Join-Path $solutionRoot "publish\PWA"

    #–– 1) Publish the PWA ––
    Write-Host "INFO: dotnet publish $pwaProj → $pwaPublish" -ForegroundColor Cyan
    dotnet publish $pwaProj -c Release -o $pwaPublish

    #–– source wwwroot of PWA ––
    $pwaWwwRoot = Join-Path $pwaPublish "wwwroot"
    if ( -not (Test-Path $pwaWwwRoot) ) {
        Write-Error "ERROR: Cannot find PWA wwwroot at $pwaWwwRoot" -ForegroundColor Red
        exit 1
    }
    Write-Host "SUCCESS: PWA published. wwwroot at $pwaWwwRoot" -ForegroundColor Green


    #–– 2) Copy PWA assets into Host’s wwwroot ––
    $hostWwwRoot = Join-Path $solutionRoot "publish\Host\wwwroot"
    Write-Host "`nINFO: Preparing Host wwwroot at $hostWwwRoot" -ForegroundColor Cyan

    # ensure host wwwroot exists and is clean
    if ( Test-Path $hostWwwRoot ) {
        Write-Host "INFO: Clearing existing files…" -ForegroundColor Cyan
        Remove-Item (Join-Path $hostWwwRoot '*') -Recurse -Force
    }
    else {
        Write-Host "INFO: Creating Host wwwroot directory…" -ForegroundColor Cyan
        New-Item -ItemType Directory -Path $hostWwwRoot | Out-Null
    }

    # copy everything from PWA/wwwroot into Host/wwwroot
    Write-Host "INFO: Copying PWA static assets into Host wwwroot…" -ForegroundColor Cyan
    Copy-Item -Path (Join-Path $pwaWwwRoot '*') -Destination $hostWwwRoot -Recurse -Force

    Write-Host "SUCCESS: All PWA assets are now in Host's wwwroot." -ForegroundColor Green
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
    $healthUrl   = "$drugApiBaseUrl/health"
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
        $drugDataUrl = "$drugApiBaseUrl/api/Jobs/import-drug-data"
        $response = Invoke-WebRequest -Uri $drugDataUrl -Method POST
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
        $interactionDataUrl = "$drugApiBaseUrl/api/Jobs/import-interaction-data"
        $response = Invoke-WebRequest -Uri $interactionDataUrl -Method POST
    } catch {
        throw "ERROR: Failed to send request to import interaction data: $($_.Exception.Message)";
    }
    if ($response.StatusCode -ne 200) {
        Write-Host "ERROR: Import interaction data failed with HTTP status code $($response.StatusCode). Stopping Installation." -ForegroundColor Red
        exit 1
    }
    Write-Host "SUCCESS: Interaction data import scheduled successfully (Status $($response.StatusCode))." -ForegroundColor Green


    Write-Host "INFO: You can view the jobs status here: $drugApiBaseUrl/hangfire" -ForegroundColor Cyan
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

    Write-Host "`nAll done! '$SqlUser' has full admin rights and ownership transferred to '$WinUser' where needed." -ForegroundColor Green
}

function Register-User {
    param(
        [Parameter(Mandatory)]
        [string]$Username,

        [Parameter(Mandatory)]
        [string]$Password
    )

    $url = "$authApiBaseUrl/Auth/Register"

    $body = @{
        username = $Username
        password = $Password
    } | ConvertTo-Json -Depth 2

    try {
        $response = Invoke-RestMethod -Uri $url -Method POST -Body $body -ContentType 'application/json'
         Write-Host "SUCCESS: User '$Username' registered successfully." -ForegroundColor Green
    }
    catch {
        if ($_.Exception.Response -ne $null) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $rawResponse = $reader.ReadToEnd()
            try {
                $errorResponse = $rawResponse | ConvertFrom-Json
                foreach ($errorItem in $errorResponse.content) {
                    Write-Host "ERROR: $($errorItem.description)" -ForegroundColor Red
                }
            }
            catch {
                Write-Host "ERROR: Failed to parse error response: $rawResponse" -ForegroundColor Red
            }
        }
        else {
            Write-Host "ERROR: HTTP request failed: $($_.Exception.Message)" -ForegroundColor Red
        }
        exit 1
    }
}


#endregion

# ---------- Script Execution ----------
Write-Host "`===========================================" -ForegroundColor DarkBlue -BackgroundColor Gray
Write-Host "`Installing PharmaTrack ..." -ForegroundColor DarkBlue -BackgroundColor Gray
Write-Host "`===========================================" -ForegroundColor DarkBlue -BackgroundColor Gray

Assert-Admin

Write-Host "`nQ: Check for prerequisites? If this is the first time running this script, you MUST do this step. [Y]es / [N]o" -ForegroundColor Magenta
$PreflightAnswer = Read-Host
if ($PreflightAnswer -match '^[Yy]$') {
    Ensure-DotNet9
    Ensure-DotNetEF
    Ensure-SqlExpressInstallation
    Ensure-SqlExpressServerUser
} else {
    Write-Host "WARNING: Skipping prerequisites checks! Installation will fail if this is the first time running this script." -ForegroundColor Yellow
}

Write-Host "`nQ: Do you wish to run database migrations? [Y]es / [N]o" -ForegroundColor Magenta
$MigrationAnswer = Read-Host
if ($MigrationAnswer -match '^[Yy]$') {
    Run-Database-Migrations
}
else {
    Write-Host "WARNING: Skipping database migrations!" -ForegroundColor Yellow
}

Deploy-PWA
Deploy-APIs

Write-Host "`nQ: Do you wish to install the Windows Application (WPF)? [Y]es / [N]o" -ForegroundColor Magenta
$WpfAnswer = Read-Host
if ($WpfAnswer -match '^[Yy]$') {
    Deploy-WPF
}
else {
    Write-Host "WARNING: Skipping Windows Application Installation!" -ForegroundColor Yellow
}

Write-Host "`nQ: Do you wish to import initial data? [Y]es / [N]o" -ForegroundColor Magenta
$ImportData = Read-Host
if ($ImportData -match '^[Yy]$') {
    Import-Initial-Data
}
else {
    Write-Host "WARNING: Skipping initial import data!" -ForegroundColor Yellow
}

Write-Host "`nQ: Setup admin user? [Y]es / [N]o" -ForegroundColor Magenta
$AdminUserAnswer = Read-Host
if ($AdminUserAnswer -match '^[Yy]$') { 
    Register-User -Username $adminUsername -Password $adminPassword
} else {
    Write-Host "WARNING: Setting up admin user!" -ForegroundColor Yellow
}

Write-Host "`===========================================" -ForegroundColor DarkBlue -BackgroundColor Gray
Write-Host "`All done!" -ForegroundColor DarkBlue -BackgroundColor Gray
Write-Host "`===========================================" -ForegroundColor DarkBlue -BackgroundColor Gray
