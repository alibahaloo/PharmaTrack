<#
.SYNOPSIS
    Publishes multiple .NET API projects and a WPF app, generates a shared self-signed cert,
    copies it into each publish folder, installs APIs as Windows Services, and creates a desktop shortcut for the WPF app.

.DESCRIPTION
    • Generates a single self-signed cert and exports it to .\certs\PharmaTrackCert.pfx
    • Publishes each API project to <scriptDir>\publish\<APIName> and installs as auto-start Windows Service
    • Publishes the WPF project to <scriptDir>\publish and creates a desktop shortcut
    • Ensures .NET 9 SDK and LocalDB instance are installed and running
    • Must be run as Administrator
#>

# Stop on any error
$ErrorActionPreference = 'Stop'

#region Configuration

# List of API projects to publish & install
$projects = @(
    @{ ProjectPath = ".\Auth.API\Auth.API.csproj";     PublishDir = ".\publish\AuthAPI";     ServiceName = "PharmaTrackAuthAPI";    DisplayName = "PharmaTrack Auth API Service";    Description = "PharmaTrack Auth.API as Windows Service" },
    @{ ProjectPath = ".\Schedule.API\Schedule.API.csproj";PublishDir = ".\publish\ScheduleAPI";ServiceName = "PharmaTrackScheduleAPI";DisplayName = "PharmaTrack Schedule API Service";Description = "PharmaTrack Schedule.API as Windows Service" },
    @{ ProjectPath = ".\Gateway.API\Gateway.API.csproj";  PublishDir = ".\publish\GatewayAPI"; ServiceName = "PharmaTrackGatewayAPI";DisplayName = "PharmaTrack Gateway API Service";Description = "PharmaTrack Gateway.API as Windows Service" },
    @{ ProjectPath = ".\Drug.API\Drug.API.csproj";      PublishDir = ".\publish\DrugAPI";     ServiceName = "PharmaTrackDrugAPI";    DisplayName = "PharmaTrack Drug API Service";      Description = "PharmaTrack Drug.API as Windows Service" },
    @{ ProjectPath = ".\Inventory.API\Inventory.API.csproj";PublishDir = ".\publish\InventoryAPI";ServiceName = "PharmaTrackInventoryAPI";DisplayName = "PharmaTrack Inventory API Service";Description = "PharmaTrack Inventory.API as Windows Service" }
)

# Certificate settings (shared by all APIs)
$certSubject    = 'CN=PharmaTrack'
$certValidYears = 1
$pfxPassword    = 'YourP@ssw0rd!'

# WPF project path
$wpfProjectPath = ".\PharmaTrack.WPF\PharmaTrack.WPF.csproj"

#endregion

function Assert-Admin {
    $identity  = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        Write-Error "This script must be run as Administrator."
        exit 1
    }
}

function Ensure-DotNet9 {
    Write-Host "🔍 Checking for .NET 9 SDK..."
    if (-not (dotnet --list-sdks | Select-String '^9\.')) {
        Write-Host "⬇️ .NET 9 SDK not found. Installing silently..."
        winget install --id Microsoft.DotNet.SDK.9 -e --silent
        Write-Host "✅ .NET 9 SDK installed."
    } else {
        Write-Host "✅ .NET 9 SDK is already installed."
    }
}

function Ensure-LocalDB {
    Write-Host "🔍 Checking for LocalDB tooling..."
    if (-not (Get-Command sqllocaldb -ErrorAction SilentlyContinue)) {
        Write-Host "⬇️ LocalDB tooling not found. Installing silently..."
        $installerUrl = 'https://download.microsoft.com/download/7/c/1/7c14e92e-bdcb-4f89-b7cf-93543e7112d1/SqlLocalDB.msi'
        $tmpPath      = Join-Path $env:TEMP 'SqlLocalDB.msi'
        Invoke-WebRequest -Uri $installerUrl -OutFile $tmpPath -UseBasicParsing
        Start-Process msiexec.exe -ArgumentList "/i `"$tmpPath`" /quiet /norestart IACCEPTSQLLOCALDBLICENSETERMS=YES" -Wait
        Write-Host "✅ LocalDB tooling installed."
    } else {
        Write-Host "✅ LocalDB tooling present."
    }

    Write-Host "🔍 Checking for default LocalDB instance 'MSSQLLocalDB'..."
    try { sqllocaldb info MSSQLLocalDB | Out-Null; Write-Host "✅ Default LocalDB instance exists." }
    catch { Write-Host "⬇️ Creating default LocalDB instance 'MSSQLLocalDB'..."; sqllocaldb create MSSQLLocalDB; Write-Host "✅ Instance created." }

    Write-Host "🚀 Starting LocalDB instance 'MSSQLLocalDB'..."
    sqllocaldb start MSSQLLocalDB
    Write-Host "✅ LocalDB instance 'MSSQLLocalDB' is running."
}

function Remove-ServiceIfExists {
    param([string]$name)
    if (Get-Service -Name $name -ErrorAction SilentlyContinue) {
        Write-Host "🛑 Stopping & deleting existing service '$name'..."
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
    Write-Host "🔧 Installing service '$name'..."
    New-Service -Name $name -BinaryPathName "`"$binPath`"" -DisplayName $display -StartupType Automatic
    Set-Service   -Name $name -Description $desc
    Start-Service -Name $name
    Write-Host "✅ Service '$name' is running."
}

# ---------- Script Execution ----------
Assert-Admin
Ensure-DotNet9
Ensure-LocalDB

Write-Host "`n🔍 Cleaning up any existing services before publish..."
foreach ($proj in $projects) { Remove-ServiceIfExists -name $proj.ServiceName }

# Determine script paths
$scriptDir      = Split-Path -Parent $MyInvocation.MyCommand.Definition
$centralCertDir = Join-Path $scriptDir 'certs'
$centralPfxPath = Join-Path $centralCertDir 'PharmaTrackCert.pfx'

Write-Host "`n📂 Creating central cert folder: $centralCertDir"
New-Item -ItemType Directory -Path $centralCertDir -Force | Out-Null

if (-not (Test-Path $centralPfxPath)) {
    Write-Host "🔐 Generating self-signed cert and exporting to PFX..."
    $cert = New-SelfSignedCertificate -Subject $certSubject -CertStoreLocation 'Cert:\LocalMachine\My' -NotAfter (Get-Date).AddYears($certValidYears)
    $securePwd = ConvertTo-SecureString -String $pfxPassword -AsPlainText -Force
    Export-PfxCertificate -Cert $cert -FilePath $centralPfxPath -Password $securePwd
    Write-Host "✅ Certificate created at $centralPfxPath"
} else { Write-Host "ℹ️  PFX exists at $centralPfxPath – skipping creation." }

# Publish and install each API
foreach ($proj in $projects) {
    $csproj   = $proj.ProjectPath
    $outDir   = Join-Path $scriptDir $proj.PublishDir
    $svcName  = $proj.ServiceName
    $dispName = $proj.DisplayName
    $desc     = $proj.Description

    Write-Host "`n=== Processing API: $csproj ==="
    Write-Host "📦 Publishing to $outDir..."
    dotnet publish $csproj -c Release -r win-x64 --self-contained true -o $outDir

    # Copy cert
    $projCert = Join-Path $outDir 'certs'
    Write-Host "📋 Copying cert to $projCert"
    New-Item -ItemType Directory -Path $projCert -Force | Out-Null
    Copy-Item -Path $centralPfxPath -Destination $projCert -Force

    # Install service
    $exeName = [IO.Path]::GetFileNameWithoutExtension($csproj) + '.exe'
    $exePath = Join-Path $outDir $exeName
    if (-not (Test-Path $exePath)) { throw "❌ Missing exe: $exePath" }
    Remove-ServiceIfExists -name $svcName
    Install-Service        -name $svcName -display $dispName -binPath $exePath -desc $desc
}

# Publish WPF app
$wpfOut = Join-Path $scriptDir 'publish\PharmaTrack.WPF'
Write-Host "`n📦 Publishing WPF app: $wpfProjectPath → $wpfOut"
dotnet publish $wpfProjectPath -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $wpfOut

# Create desktop shortcut
Write-Host "🔗 Creating desktop shortcut for WPF app"
$desktop     = [Environment]::GetFolderPath('Desktop')
$shortcut    = Join-Path $desktop 'PharmaTrack.lnk'
$targetExe   = Join-Path $wpfOut 'PharmaTrack.WPF.exe'
$wshell      = New-Object -ComObject WScript.Shell
$link        = $wshell.CreateShortcut($shortcut)
$link.TargetPath      = $targetExe
$link.WorkingDirectory= Split-Path $targetExe
$link.WindowStyle     = 1
$link.Description     = 'PharmaTrack WPF App'
$link.Save()

Write-Host "`n🎉 All APIs published, services installed, WPF app deployed, and shortcut created!"
