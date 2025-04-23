# install-pharmatrack.ps1
# Orchestrates installation of PharmaTrack: publishes WPF app, generates certificates,
# deploys Docker containers, import needed data, and creates a desktop shortcut.

Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "Installing PharmaTrack..." -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# --- Pre-flight Checks ---
# Ensure script is run as Administrator
if (-not (([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))) {
    Write-Error "ERROR: This script must be run as Administrator."
    Exit 1
}

# PowerShell version
if ($PSVersionTable.PSVersion.Major -lt 5) {
    Write-Error "ERROR: PowerShell 5.1+ required. Current: $($PSVersionTable.PSVersion)"
    Exit 1
}

# OpenSSL + Chocolatey installer
Write-Host "`nChecking for OpenSSL..." -ForegroundColor Cyan
if (Get-Command openssl -ErrorAction SilentlyContinue) {
    Write-Host "INFO: OpenSSL is already installed." -ForegroundColor Green
} else {
    Write-Host "WARNING: OpenSSL not found. Attempting to install via Chocolatey..." -ForegroundColor Yellow

    # 1) Ensure Chocolatey
    if (-not (Get-Command choco -ErrorAction SilentlyContinue)) {
        Write-Host "  • Chocolatey not found. Installing Chocolatey..." -ForegroundColor Yellow

        # allow script execution & enforce TLS1.2
        Set-ExecutionPolicy Bypass -Scope Process -Force
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

        # install script from official source
        iex ((New-Object Net.WebClient).DownloadString(
          'https://community.chocolatey.org/install.ps1'
        ))

        # add choco to PATH for this session
        $chocoPath = (Get-ItemProperty 'HKLM:\Software\Chocolatey\InstallLocation').InstallLocation + '\bin'
        $env:Path += ";$chocoPath"
        Write-Host "  ✓ Chocolatey installed." -ForegroundColor Green
    } else {
        Write-Host "  ✓ Chocolatey is already installed." -ForegroundColor Green
    }

    # 2) Install OpenSSL
    Write-Host "  • Installing OpenSSL via choco..." -ForegroundColor Yellow
    choco install openssl.light -y
    if ($LASTEXITCODE -ne 0) {
        Write-Error "ERROR: Failed to install OpenSSL via Chocolatey."
        Exit 1
    }

    # refresh environment if RefreshEnv is available
    if (Get-Command RefreshEnv -ErrorAction SilentlyContinue) {
        RefreshEnv | Out-Null
    }

    # final check
    if (Get-Command openssl -ErrorAction SilentlyContinue) {
        Write-Host "  ✓ OpenSSL installed successfully." -ForegroundColor Green
    } else {
        Write-Error "ERROR: OpenSSL still not found after installation."
        Exit 1
    }
}

# Ensure Docker Engine is running
& docker info > $null 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "ERROR: Docker engine is not running or reachable."
    Exit 1
} 

# Check for Docker Compose plugin
if (-not (Get-Command docker-compose -ErrorAction SilentlyContinue) -and
    -not (Get-Command docker -ErrorAction SilentlyContinue | Where Name -eq 'compose')) {
    Write-Error "ERROR: Docker Compose not found."
    Exit 1
}

# .NET CLI
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "ERROR: .NET SDK (dotnet CLI) not found."
    Exit 1
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
Write-Host "INFO: Import drug data completed successfully (Status $($response.StatusCode))." -ForegroundColor Green

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
Write-Host "INFO: Import interaction data completed successfully (Status $($response.StatusCode))." -ForegroundColor Green

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
