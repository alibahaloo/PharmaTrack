# deploy.ps1
# ----------------
# Trust the Root CA and spin up Docker.

# Step 1: Trust the Root CA in Windows
Write-Host "Trusting Root CA…" -ForegroundColor Cyan
Import-Certificate `
  -FilePath "$PSScriptRoot\rootCA.crt" `
  -CertStoreLocation Cert:\LocalMachine\Root

# Step 2: Deploy Docker containers
Write-Host "Deploying Docker containers…" -ForegroundColor Cyan
docker-compose down
docker-compose up -d

Write-Host "Docker services are up and running." -ForegroundColor Green
