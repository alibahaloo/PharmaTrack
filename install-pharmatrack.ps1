# Stop on any error
$ErrorActionPreference = 'Stop'

function Install-Chocolatey {
    if (-not (Get-Command choco -ErrorAction SilentlyContinue)) {
        Write-Host "INFO: Chocolatey not found. Installing..." -ForegroundColor Cyan
        Set-ExecutionPolicy Bypass -Scope Process -Force
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        iex ((New-Object Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
        $chocoBin = (Get-ItemProperty 'HKLM:\Software\Chocolatey\InstallLocation').InstallLocation + '\bin'
        $env:Path += ";$chocoBin"
        Write-Host "INFO: Chocolatey installed." -ForegroundColor Green
    } else {
        Write-Host "INFO: Chocolatey is already installed." -ForegroundColor Green
    }
}

function Assert-Admin {
    if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        Write-Host "ERROR: This script must be run as Administrator." -ForegroundColor Red
        Exit 1
    }
    Write-Host "INFO: Running as Administrator." -ForegroundColor Green
}

function Ensure-DockerEngine {
    # —–– remove stale docker-feedback plugin so no spurious errors
    $plugin = Join-Path $env:USERPROFILE '.docker\cli-plugins\docker-feedback.exe'
    if (Test-Path $plugin) {
        Write-Host "INFO: Removing stale Docker CLI plugin 'docker-feedback.exe'…" -ForegroundColor Yellow
        Remove-Item $plugin -Force
    }

    # —–– start the Docker Windows service if it's not already running
    $svc = Get-Service -Name 'com.docker.service','Docker' -ErrorAction SilentlyContinue |
           Where-Object Status -ne 'Running' |
           Select-Object -First 1
    if ($svc) {
        Write-Host "INFO: Starting Docker service '$($svc.Name)'…" -ForegroundColor Cyan
        Start-Service -Name $svc.Name
        try {
            # wait up to 30 seconds for the service to become Running
            $svc.WaitForStatus('Running','00:00:30')
        } catch {
            throw "ERROR: Docker service '$($svc.Name)' failed to start within 30 seconds."
        }
        Write-Host "INFO: Docker service is now running." -ForegroundColor Green
    }

    # —–– temporarily allow non-terminating errors so we can swallow warnings
    $oldEAP = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'

    # —–– invoke docker info, suppress both stdout and stderr
    & docker info > $null 2>&1

    # —–– restore original error action preference
    $ErrorActionPreference = $oldEAP

    # —–– if docker still isn't reachable, exit with error
    if ($LASTEXITCODE -ne 0) {
        throw "ERROR: Docker engine is still unreachable (exit code $LASTEXITCODE)."
    }

    Write-Host "INFO: Docker engine is up and responsive." -ForegroundColor Green
}

function Assert-DockerCompose {
    $hasComposeLegacy = Get-Command docker-compose -ErrorAction SilentlyContinue
    $composePluginOk = $false
    try { docker compose version > $null 2>&1; $composePluginOk = $true } catch {}
    if (-not $hasComposeLegacy -and -not $composePluginOk) {
        Write-Host "ERROR: Docker Compose not found." -ForegroundColor Red
        Exit 1
    }
    Write-Host "INFO: Docker Compose is available." -ForegroundColor Green
}

function Ensure-DotNetSdk {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-Host "WARNING: .NET CLI not found. Installing .NET 7.0 SDK via Chocolatey..." -ForegroundColor Yellow
        Install-Chocolatey
        choco install dotnet-7.0-sdk -y --no-progress
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ERROR: Failed to install .NET SDK." -ForegroundColor Red
            Exit 1
        }
        if (Get-Command RefreshEnv -ErrorAction SilentlyContinue) { RefreshEnv | Out-Null }
        Write-Host "INFO: .NET SDK installed successfully." -ForegroundColor Green
    } else {
        Write-Host "INFO: .NET CLI detected (v$(dotnet --version))." -ForegroundColor Green
    }
}

function Ensure-OpenSSL {
    if (-not (Get-Command openssl -ErrorAction SilentlyContinue)) {
        Write-Host "WARNING: OpenSSL not found. Installing via Chocolatey..." -ForegroundColor Yellow
        Install-Chocolatey
        choco install openssl.light -y --no-progress
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ERROR: Failed to install OpenSSL." -ForegroundColor Red
            Exit 1
        }
        if (Get-Command RefreshEnv -ErrorAction SilentlyContinue) { RefreshEnv | Out-Null }
        Write-Host "INFO: OpenSSL installed successfully." -ForegroundColor Green
    } else {
        Write-Host "INFO: OpenSSL is already installed." -ForegroundColor Green
    }
}

# --- Pre-Flight ---
Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "Installing PharmaTrack - Pre-Flight Checks" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

Assert-Admin
Ensure-DockerEngine
Assert-DockerCompose
Ensure-DotNetSdk
Ensure-OpenSSL

# Validate required files
$required = @(
    "$PSScriptRoot\PharmaTrack.WPF\PharmaTrack.WPF.csproj",
    "$PSScriptRoot\generate-certs.ps1",
    "$PSScriptRoot\deploy.ps1",
    "$PSScriptRoot\compose.yaml"
)
foreach ($f in $required) {
    if (-not (Test-Path $f)) {
        Write-Host "ERROR: Required file not found: $f" -ForegroundColor Red
        Exit 1
    }
}

# Step 1: Publish the WPF app (self-contained, single file)
Write-Host "`nStep 1: Publishing WPF app..." -ForegroundColor Cyan
dotnet publish "$PSScriptRoot\PharmaTrack.WPF\PharmaTrack.WPF.csproj" `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -o "$PSScriptRoot\publish"

# Step 2: Generate and export certs
Write-Host "`nStep 2: Generating certificates..." -ForegroundColor Cyan
& "$PSScriptRoot\generate-certs.ps1"

# Step 3: Trust cert + start Docker containers
Write-Host "`nStep 3: Deploying Docker containers..." -ForegroundColor Cyan
& "$PSScriptRoot\deploy.ps1"

# Step 4: Import initial data via API endpoints
Write-Host "`nStep 4: Importing initial data into the system..."

# — wait for API port to be open before returning —
Write-Host "`nStep 4a: Waiting for API readiness via /health..." -ForegroundColor Cyan

$healthUrl   = 'http://localhost:8089/health'
$maxRetries  = 12          # e.g. 12 attempts
$delaySecs   = 5           # 5 seconds between tries
$attempt     = 0

do {
    $attempt++
    try {
        $resp = Invoke-WebRequest -Uri $healthUrl -Method GET -TimeoutSec 5 -ErrorAction Stop
        if ($resp.StatusCode -eq 200) {
            Write-Host "`nINFO: API is healthy (after $attempt attempt(s))." -ForegroundColor Green
            break
        }
    } catch {
        Write-Host -NoNewline "."
        Start-Sleep -Seconds $delaySecs
    }
    if ($attempt -ge $maxRetries) {
        throw "ERROR: API health check failed: $healthUrl did not return 200 after $maxRetries attempts."
    }
} while ($true)

# Now that /health is green, do your imports once
Write-Host "`nStep 4b: Importing initial data via API endpoints..." -ForegroundColor Cyan

# POST to import drug data
try {
    $response = Invoke-WebRequest -Uri "http://localhost:8089/api/Jobs/import-drug-data" -Method POST
} catch {
    throw "ERROR: Failed to send request to import drug data: $($_.Exception.Message)"
}
if ($response.StatusCode -ne 200) {
    Write-Host "ERROR: Import drug data failed with HTTP status code $($response.StatusCode)."
    throw "ERROR: Import drug data returned HTTP $($response.StatusCode). Stopping installation."
}
Write-Host "INFO: Drug data import scheduled successfully (Status $($response.StatusCode))." -ForegroundColor Green

# POST to import interaction data
try {
    $response = Invoke-WebRequest -Uri "http://localhost:8089/api/Jobs/import-interaction-data" -Method POST
} catch {
    throw "ERROR: Failed to send request to import interaction data: $($_.Exception.Message)"
}
if ($response.StatusCode -ne 200) {
    Write-Host "ERROR: Import interaction data failed with HTTP status code $($response.StatusCode)."
    throw "ERROR: Import interaction data returned HTTP $($response.StatusCode). Stopping installation."
}
Write-Host "INFO: Interaction data import scheduled successfully (Status $($response.StatusCode))." -ForegroundColor Green

Write-Host "INFO: You can view the jobs status here: http://localhost:8089/hangfire" -ForegroundColor Green

# Step 5: Create a Desktop shortcut for WPF app
Write-Host "`nStep 5: Creating Desktop shortcut..." -ForegroundColor Cyan
$shortcutPath = "$([Environment]::GetFolderPath('Desktop'))\PharmaTrack.lnk"
$targetPath   = "$PSScriptRoot\publish\PharmaTrack.WPF.exe"

$WScriptShell = New-Object -ComObject WScript.Shell
$Shortcut     = $WScriptShell.CreateShortcut($shortcutPath)
$Shortcut.TargetPath      = $targetPath
$Shortcut.WorkingDirectory = (Split-Path $targetPath)
$Shortcut.WindowStyle      = 1
$Shortcut.Description      = "PharmaTrack WPF App"
$Shortcut.Save()

Write-Host "`n======================================================================" -ForegroundColor Green
Write-Host "Installation complete! You can now open the application." -ForegroundColor Green
Write-Host "======================================================================" -ForegroundColor Green
