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

function Ensure-LocalDB {
    Write-Host "INFO: Checking for LocalDB tooling..." -ForegroundColor Cyan
    if (-not (Get-Command sqllocaldb -ErrorAction SilentlyContinue)) {
        Write-Host "WARNING: LocalDB tooling not found. Installing silently..." -ForegroundColor Yellow
        $installerUrl = 'https://download.microsoft.com/download/7/c/1/7c14e92e-bdcb-4f89-b7cf-93543e7112d1/SqlLocalDB.msi'
        $tmpPath      = Join-Path $env:TEMP 'SqlLocalDB.msi'
        Invoke-WebRequest -Uri $installerUrl -OutFile $tmpPath -UseBasicParsing
        Start-Process msiexec.exe -ArgumentList "/i `"$tmpPath`" /quiet /norestart IACCEPTSQLLOCALDBLICENSETERMS=YES" -Wait
        Write-Host "SUCCESS: LocalDB tooling installed." -ForegroundColor Green
    } else {
        Write-Host "SUCCESS: LocalDB tooling present." -ForegroundColor Green
    }
    Write-Host "INFO: Ensuring default LocalDB instance 'MSSQLLocalDB' exists and is running..." -ForegroundColor Cyan
    try { sqllocaldb info MSSQLLocalDB | Out-Null } catch { sqllocaldb create MSSQLLocalDB }
    sqllocaldb start MSSQLLocalDB | Out-Null
    Write-Host "SUCCESS: LocalDB instance 'MSSQLLocalDB' is running." -ForegroundColor Green
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

#endregion

# ---------- Script Execution ----------
Assert-Admin
Ensure-DotNet9
Ensure-LocalDB

Deploy-Certificates
Deploy-APIs
Deploy-WPF
Import-Initial-Data
