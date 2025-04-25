# Stop on any error
$ErrorActionPreference = 'Stop'

function Install-Chocolatey {
    if (-not (Get-Command choco -ErrorAction SilentlyContinue)) {
        Write-Host "INFO: Chocolatey not found. Installing..." -ForegroundColor Cyan
        Set-ExecutionPolicy Bypass -Scope Process -Force
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        iex ((New-Object Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
        # Load the freshly created machine environment variable into this session
        $env:ChocolateyInstall = [Environment]::GetEnvironmentVariable('ChocolateyInstall','Machine')
        $chocoBin = Join-Path $env:ChocolateyInstall 'bin'
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
    Write-Host "INFO: Installing Docker Engine (Moby) via Chocolatey..." -ForegroundColor Cyan
    # Use Chocolatey as a reliable fallback on this platform
    Install-Chocolatey
    choco install docker-engine -y --no-progress
    choco install docker-cli -y --no-progress
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to install Docker Engine or CLI." -ForegroundColor Red
        Exit 1
    }
    # Refresh environment if available to pick up newly installed executables
    if (Get-Command RefreshEnv -ErrorAction SilentlyContinue) { RefreshEnv | Out-Null }
    # Start & configure the Docker service (if the package provided it)
    try {
        Start-Service docker
        Set-Service docker -StartupType Automatic
    } catch {
        Write-Warning "Docker service not found or already running."
    }
    # Verify the Docker CLI is available
    & docker version > $null 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "ERROR: Docker CLI still not responding (exit code $LASTEXITCODE)."
    }
    Write-Host "INFO: Docker engine is installed and running." -ForegroundColor Green
}


function Ensure-DockerCompose {
    Write-Host "INFO: Ensuring Docker Compose plugin is available..." -ForegroundColor Cyan
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

    $pluginDir = "$Env:ProgramFiles\Docker\cli-plugins"
    $pluginPath = Join-Path $pluginDir 'docker-compose.exe'

    # If plugin exists and is functional, skip download
    if (Test-Path $pluginPath) {
        try {
            docker compose version > $null 2>&1
            Write-Host "INFO: Docker Compose plugin already installed." -ForegroundColor Green
        } catch {
            Write-Host "WARNING: Docker Compose plugin found but failed to run; reinstalling." -ForegroundColor Yellow
            Remove-Item $pluginPath -Force
        }
    }

    # Download plugin if missing
    if (-not (Test-Path $pluginPath)) {
        Write-Host "INFO: Installing Docker Compose plugin..." -ForegroundColor Cyan
        try {
            $release = Invoke-RestMethod -UseBasicParsing "https://api.github.com/repos/docker/compose/releases/latest"
            $tag    = $release.tag_name
            if (-not (Test-Path $pluginDir)) { New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null }
            $url = "https://github.com/docker/compose/releases/download/$tag/docker-compose-windows-x86_64.exe"
            Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile $pluginPath
        } catch {
            Write-Error "Failed to download Docker Compose plugin: $_"
            Exit 1
        }
    }

    # Verify plugin works
    try {
        docker compose version > $null 2>&1
    } catch {
        Write-Error "ERROR: Docker Compose plugin not responding (exit code $LASTEXITCODE)."
        Exit 1
    }
    Write-Host "INFO: Docker Compose plugin installed and operational." -ForegroundColor Green

    # Create docker-compose standalone shim in PATH if not present
    $chocoBin = Join-Path $Env:ChocolateyInstall 'bin'
    $standalone = Join-Path $chocoBin 'docker-compose.exe'
    if (-not (Test-Path $standalone)) {
        Copy-Item -Path $pluginPath -Destination $standalone -Force
        Write-Host "INFO: Created docker-compose shim at $standalone" -ForegroundColor Green
    }
}

function Ensure-DotNetSdk {
    Write-Host "INFO: Installing .NET 9.0 SDK via Chocolatey..." -ForegroundColor Cyan
    Install-Chocolatey
    choco install dotnet-9.0-sdk -y --no-progress
    $exitCode = $LASTEXITCODE
    if ($exitCode -eq 3010) {
        Write-Host "WARNING: .NET 9.0 SDK installed; reboot recommended (exit code 3010)." -ForegroundColor Yellow
    } elseif ($exitCode -ne 0) {
        Write-Host "ERROR: Failed to install .NET 9.0 SDK (exit code $exitCode)." -ForegroundColor Red
        Exit 1
    }
    # Refresh environment so 'dotnet' is available immediately
    if (Get-Command RefreshEnv -ErrorAction SilentlyContinue) { RefreshEnv | Out-Null }
    Write-Host "INFO: .NET 9.0 SDK installed successfully (v$(dotnet --version))." -ForegroundColor Green
}

function Ensure-OpenSSL {
    $opensslPath = "C:\\Program Files\\OpenSSL\\bin\\openssl.exe"
    if (-not (Test-Path $opensslPath)) {
        Write-Host "WARNING: OpenSSL not found. Installing via Chocolatey..." -ForegroundColor Yellow
        Install-Chocolatey
        choco install openssl.light -y --no-progress
        $exitCode = $LASTEXITCODE
        if ($exitCode -eq 3010) {
            Write-Host "WARNING: OpenSSL installed; reboot recommended (exit code 3010)." -ForegroundColor Yellow
        } elseif ($exitCode -ne 0) {
            Write-Host "ERROR: Failed to install OpenSSL (exit code $exitCode)." -ForegroundColor Red
            Exit 1
        }
    } else {
        Write-Host "INFO: OpenSSL binary already present." -ForegroundColor Green
    }
    # Ensure the bin path is on PATH
    $opensslDir = Split-Path $opensslPath
    if ($env:Path -notmatch [regex]::Escape($opensslDir)) {
        $env:Path += ";$opensslDir"
    }
    # Verify openssl works now
    & "$opensslPath" version > $null 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: OpenSSL executable not responding (exit code $LASTEXITCODE)." -ForegroundColor Red
        Exit 1
    }
    Write-Host "INFO: OpenSSL is installed and operational." -ForegroundColor Green
}

# --- Pre-Flight ---
Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "Installing PharmaTrack - Pre-Flight Checks" -ForegroundColor Cyan
Write-Host "================================================`n" -ForegroundColor Cyan

Assert-Admin
Ensure-DockerEngine
Ensure-DockerCompose
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
# Step 3.1: Trust the Root CA in Windows
Write-Host "Trusting Root CA…" -ForegroundColor Cyan
Import-Certificate `
  -FilePath "$PSScriptRoot\rootCA.crt" `
  -CertStoreLocation Cert:\LocalMachine\Root

# Step 3.2: Deploy Docker containers
Write-Host "Deploying Docker containers…" -ForegroundColor Cyan
docker-compose down
docker-compose build
docker-compose up -d

Write-Host "INFO: Docker services are deployed and running." -ForegroundColor Green

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
