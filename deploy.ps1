<#
.SYNOPSIS
    Publishes multiple .NET API projects and installs each as a Windows Service.
.DESCRIPTION
    • Configurable list of projects, their publish folders, and service metadata.
    • Builds each project as a self-contained executable and outputs to its publish directory.
    • Registers each published exe as a Windows Service that auto-starts on boot.
    • Must be run as Administrator. Terminates on any failure.
#>

# Treat all errors as terminating
$ErrorActionPreference = 'Stop'

#region Configuration
# Define your projects here:
$projects = @(
    @{ 
        ProjectPath = ".\Auth.API\Auth.API.csproj";
        PublishDir  = ".\publish\AuthAPI";
        ServiceName = "AuthAPI";
        DisplayName = "Auth API Service";
        Description = "Self-contained ASP.NET Core Auth.API as Windows Service"
    },
    @{ 
        ProjectPath = ".\Schedule.API\Schedule.API.csproj";
        PublishDir  = ".\publish\ScheduleAPI";
        ServiceName = "ScheduleAPI";
        DisplayName = "Schedule API Service";
        Description = "Self-contained ASP.NET Core Schedule.API as Windows Service"
    },
    @{ 
        ProjectPath = ".\Gateway.API\Gateway.API.csproj";
        PublishDir  = ".\publish\GatewayAPI";
        ServiceName = "GatewayAPI";
        DisplayName = "Gateway API Service";
        Description = "Self-contained ASP.NET Core Gateway.API as Windows Service"
    },
    @{ 
        ProjectPath = ".\Drug.API\Drug.API.csproj";
        PublishDir  = ".\publish\DrugAPI";
        ServiceName = "DrugAPI";
        DisplayName = "Drug API Service";
        Description = "Self-contained ASP.NET Core Drug.API as Windows Service"
    },
    @{ 
        ProjectPath = ".\Inventory.API\Inventory.API.csproj";
        PublishDir  = ".\publish\InventoryAPI";
        ServiceName = "InventoryAPI";
        DisplayName = "Inventory API Service";
        Description = "Self-contained ASP.NET Core Inventory.API as Windows Service"
    }
)
#endregion

function Assert-Admin {
    $current   = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($current)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        Write-Error "This script must be run as Administrator."
        exit 1
    }
}

function Remove-ServiceIfExists {
    param([string]$name)
    if (Get-Service -Name $name -ErrorAction SilentlyContinue) {
        Write-Host "Stopping and deleting existing service '$name'..."
        Stop-Service  -Name $name -Force -ErrorAction SilentlyContinue
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
    try {
        Write-Host "Creating service '$name'..."
        New-Service -Name $name -BinaryPathName "`"$binPath`"" -DisplayName $display -StartupType Automatic -ErrorAction Stop
        Write-Host "Setting description for '$name'..."
        Set-Service -Name $name -Description $desc -ErrorAction Stop
        Write-Host "Starting service '$name'..."
        Start-Service -Name $name -ErrorAction Stop
        Write-Host "✅ Service '$name' installed and started successfully."
    }
    catch {
        Write-Error "Failed to install or start service '$name': $_"
        throw
    }
}

# ---------- Script Execution ----------
try {
    Assert-Admin
    foreach ($proj in $projects) {
        $path      = $proj.ProjectPath
        $out       = $proj.PublishDir
        $svc       = $proj.ServiceName
        $disp      = $proj.DisplayName
        $desc      = $proj.Description

        Write-Host "`n=== Processing project: $path ==="

        # Publish step
        Write-Host "Publishing '$path' to '$out'..."
        dotnet publish $path -c Release -r win-x64 --self-contained true -o $out

        # Verify publish
        $exeName = [System.IO.Path]::GetFileNameWithoutExtension($path) + ".exe"
        $exePath = Join-Path $out $exeName
        if (-not (Test-Path $exePath)) {
            throw "Published executable not found: $exePath"
        }

        # Install as Windows Service
        Remove-ServiceIfExists -name $svc
        Install-Service        -name $svc -display $disp -binPath $exePath -desc $desc
    }
    Write-Host "`nAll projects processed successfully."
}
catch {
    Write-Error "Script aborted due to error: $_"
    exit 1
}
