# Self-elevate if not already running as Administrator
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(`
    [Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Start-Process powershell "-NoExit -ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    exit
}

# Step 6: Trust the Root CA in Windows
Import-Certificate -FilePath "$PSScriptRoot\rootCA.crt" `
  -CertStoreLocation Cert:\LocalMachine\Root

# Step 7: Ensure your docker-compose.yml includes:
#   volumes:
#     - ./aspnetapp.pfx:/https/aspnetapp.pfx:ro
#   environment:
#     - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
#     - ASPNETCORE_Kestrel__Certificates__Default__Password=SuperSecretP@ss

# Step 8: Deploy Docker containers
docker-compose down
docker-compose up -d