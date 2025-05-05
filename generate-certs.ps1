# === Certificate Generation Script ===

Write-Host "Generating Dev Certificats (to export to Docker)... " -ForegroundColor Cyan

New-Item -ItemType Directory -Path "./devCerts" -Force

dotnet dev-certs https --trust

dotnet dev-certs https -ep ./devCerts/aspnetapp.pfx -p YourP@ssw0rd!

Write-Host "SUCCESS: Certificates generated and ready to be exported." -ForegroundColor Green
