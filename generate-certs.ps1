# === Save & Run Instructions ===
# 1. Open PowerShell as Administrator and cd into this folder.
# 2. Run:
#      Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
#      .\generate-certs.ps1

# === Certificate Generation Script ===

# Step 0: Write OpenSSL SAN configuration (needed by *all* openssl req calls)
@"
[req]
default_bits       = 2048
prompt             = no
default_md         = sha256
distinguished_name = dn
req_extensions     = req_ext

[dn]
C  = CA
ST = BC
L  = North Vancouver
O  = PharmaTrack
CN = localhost

[req_ext]
subjectAltName = @alt_names

[alt_names]
DNS.1 = localhost
IP.1  = 127.0.0.1
"@ | Out-File -Encoding ascii "$PSScriptRoot\openssl-san.cnf"

# Step 1: Generate Root CA private key and self-signed certificate
Write-Host "Generating Root CA…" -ForegroundColor Cyan
openssl genrsa -out rootCA.key 4096
openssl req -x509 -new -nodes `
  -config "$PSScriptRoot\openssl-san.cnf" `
  -key rootCA.key `
  -sha256 -days 3650 `
  -subj "/C=CA/ST=BC/L=North Vancouver/O=PharmaTrack/CN=PharmaTrack Local Root CA" `
  -out rootCA.crt

# Step 2: Generate server private key and CSR
Write-Host "Generating server CSR…" -ForegroundColor Cyan
openssl req -new -nodes `
  -config "$PSScriptRoot\openssl-san.cnf" `
  -out server.csr `
  -newkey rsa:2048 `
  -keyout server.key

# Step 3: Sign the CSR with your Root CA to create the server certificate
Write-Host "Signing server certificate…" -ForegroundColor Cyan
openssl x509 -req `
  -in server.csr `
  -CA rootCA.crt `
  -CAkey rootCA.key `
  -CAcreateserial `
  -out server.crt `
  -days 730 `
  -sha256 `
  -extfile "$PSScriptRoot\openssl-san.cnf" `
  -extensions req_ext

# Step 4: Export the server key and full chain into a PFX for Kestrel
Write-Host "Exporting PFX…" -ForegroundColor Cyan
openssl pkcs12 -export `
  -out aspnetapp.pfx `
  -inkey server.key `
  -in server.crt `
  -certfile rootCA.crt `
  -passout pass:SuperSecretP@ss

Write-Host "Certificates generated." -ForegroundColor Green
