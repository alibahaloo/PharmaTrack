# Step 0: Self-elevate if not already Admin
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Start-Process powershell "-NoExit -ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

Write-Host "`nInstalling PharmaTrack..." -ForegroundColor Cyan

# Step 1: Publish the WPF app (self-contained, single file)
Write-Host "`nPublishing WPF app..." -ForegroundColor Cyan
dotnet publish .\PharmaTrack.WPF\PharmaTrack.WPF.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -o .\publish

# Step 2: Generate and export certs
Write-Host "`nGenerating certificates..." -ForegroundColor Cyan
.\generate-certs.ps1

# Step 3: Trust cert + start Docker containers
Write-Host "`nDeploying Docker containers..." -ForegroundColor Cyan
.\deploy.ps1

# Step 4: Create a Desktop shortcut for WPF app
Write-Host "`nCreating Desktop shortcut..." -ForegroundColor Cyan
$shortcutPath = "$([Environment]::GetFolderPath('Desktop'))\PharmaTrack.lnk"
$targetPath = "$PSScriptRoot\publish\PharmaTrack.WPF.exe"

$WScriptShell = New-Object -ComObject WScript.Shell
$Shortcut = $WScriptShell.CreateShortcut($shortcutPath)
$Shortcut.TargetPath = $targetPath
$Shortcut.WorkingDirectory = (Split-Path $targetPath)
$Shortcut.WindowStyle = 1
$Shortcut.Description = "PharmaTrack WPF App"
$Shortcut.Save()

Write-Host "`nInstallation complete! You can now launch PharmaTrack from your Desktop." -ForegroundColor Green
