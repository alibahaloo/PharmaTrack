<#
.SYNOPSIS
    Publishes multiple .NET API projects, generates one shared self-signed cert,
    copies it into each publish folder, and installs each as a Windows Service.

.DESCRIPTION
    • Generates a single self-signed cert and exports it to .\certs\AuthApiService.pfx
    • Publishes each project and copies the shared PFX into <PublishDir>\certs\
    • Registers each published exe as an auto-start Windows Service
    • Must be run as Administrator
#>

# Stop on any error
$ErrorActionPreference = 'Stop'

#region Configuration

# List of projects to publish & install
$projects = @(
    @{ ProjectPath = ".\Auth.API\Auth.API.csproj";       PublishDir = ".\publish\AuthAPI";      ServiceName = "PharmaTrackAuthAPI";      DisplayName = "PharmaTrack Auth API Service";      Description = "PharmaTrack Auth.API as Windows Service" },
    @{ ProjectPath = ".\Schedule.API\Schedule.API.csproj"; PublishDir = ".\publish\ScheduleAPI"; ServiceName = "PharmaTrackScheduleAPI"; DisplayName = "PharmaTrack Schedule API Service"; Description = "PharmaTrack Schedule.API as Windows Service" },
    @{ ProjectPath = ".\Gateway.API\Gateway.API.csproj";   PublishDir = ".\publish\GatewayAPI";  ServiceName = "PharmaTrackGatewayAPI";  DisplayName = "PharmaTrack Gateway API Service";   Description = "PharmaTrack Gateway.API as Windows Service" },
    @{ ProjectPath = ".\Drug.API\Drug.API.csproj";         PublishDir = ".\publish\DrugAPI";     ServiceName = "PharmaTrackDrugAPI";     DisplayName = "PharmaTrack Drug API Service";      Description = "PharmaTrack Drug.API as Windows Service" },
    @{ ProjectPath = ".\Inventory.API\Inventory.API.csproj"; PublishDir = ".\publish\InventoryAPI"; ServiceName = "PharmaTrackInventoryAPI"; DisplayName = "PharmaTrack Inventory API Service"; Description = "PharmaTrack Inventory.API as Windows Service" }
)

# Certificate settings (shared by all APIs)
$certSubject    = 'CN=PharmaTrack'
$certValidYears = 1
$pfxPassword    = 'YourP@ssw0rd!'

#endregion

function Assert-Admin {
    $identity  = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        Write-Error "This script must be run as Administrator."
        exit 1
    }
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

# Determine script & cert paths
$scriptDir        = Split-Path -Parent $MyInvocation.MyCommand.Definition
$centralCertDir   = Join-Path $scriptDir 'certs'
$centralPfxPath   = Join-Path $centralCertDir 'PharmaTrackCert.pfx'

Write-Host "`n📂 Creating central cert folder: $centralCertDir"
New-Item -ItemType Directory -Path $centralCertDir -Force | Out-Null

if (-not (Test-Path $centralPfxPath)) {
    Write-Host "🔐 Generating self-signed cert and exporting to PFX..."
    $cert = New-SelfSignedCertificate `
        -Subject         $certSubject `
        -CertStoreLocation 'Cert:\LocalMachine\My' `
        -NotAfter        (Get-Date).AddYears($certValidYears)

    $securePwd = ConvertTo-SecureString -String $pfxPassword -AsPlainText -Force
    Export-PfxCertificate `
        -Cert     $cert `
        -FilePath $centralPfxPath `
        -Password $securePwd

    Write-Host "✅ Certificate created at $centralPfxPath"
} else {
    Write-Host "ℹ️  PFX already exists at $centralPfxPath – skipping creation."
}

foreach ($proj in $projects) {
    $csproj    = $proj.ProjectPath
    $outDir    = Join-Path $scriptDir $proj.PublishDir
    $svcName   = $proj.ServiceName
    $dispName  = $proj.DisplayName
    $desc      = $proj.Description

    Write-Host "`n=== Processing $csproj ==="

    # 1) Publish
    Write-Host "📦 Publishing to $outDir..."
    dotnet publish $csproj -c Release -r win-x64 --self-contained true -o $outDir

    # 2) Copy the shared cert PFX into <publish>\certs\
    $projCertDir = Join-Path $outDir 'certs'
    Write-Host "📁 Ensuring cert folder in publish output: $projCertDir"
    New-Item -ItemType Directory -Path $projCertDir -Force | Out-Null

    Write-Host "📋 Copying shared PFX → $projCertDir"
    Copy-Item -Path $centralPfxPath -Destination $projCertDir -Force

    # 3) Install as service
    $exeName = [System.IO.Path]::GetFileNameWithoutExtension($csproj) + '.exe'
    $exePath = Join-Path $outDir $exeName

    if (-not (Test-Path $exePath)) {
        throw "❌ Could not find published exe: $exePath"
    }

    Remove-ServiceIfExists -name $svcName
    Install-Service        -name $svcName -display $dispName -binPath $exePath -desc $desc
}

Write-Host "`n🎉 All APIs published, cert deployed, and services installed." 
