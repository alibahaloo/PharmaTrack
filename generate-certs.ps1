# === Save & Run Instructions ===
# 1. Open PowerShell as Administrator and cd into this folder.
# 2. Create a new file named 'generate-certs.ps1', paste everything below into it, then save.
# 3. In the same directory, run:
#      Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
#      .\generate-certs.ps1

# === Certificate Generation Script ===
# Step 1: Generate Root CA private key and self-signed certificate
openssl genrsa -out rootCA.key 4096
openssl req -x509 -new -nodes `
  -key rootCA.key `
  -sha256 -days 3650 `
  -subj "/C=CA/ST=BC/L=North Vancouver/O=PharmaTrack/CN=PharmaTrack Local Root CA" `
  -out rootCA.crt

# Step 2: Create OpenSSL SAN configuration file
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
"@ | Out-File -Encoding ascii openssl-san.cnf

# Step 3: Generate server private key and CSR
openssl req -new -nodes `
  -out server.csr `
  -newkey rsa:2048 `
  -keyout server.key `
  -config openssl-san.cnf

# Step 4: Sign the CSR with your Root CA to create the server certificate
openssl x509 -req `
  -in server.csr `
  -CA rootCA.crt `
  -CAkey rootCA.key `
  -CAcreateserial `
  -out server.crt `
  -days 730 `
  -sha256 `
  -extfile openssl-san.cnf `
  -extensions req_ext

# Step 5: Export the server key and full chain into a PFX for Kestrel
openssl pkcs12 -export `
  -out aspnetapp.pfx `
  -inkey server.key `
  -in server.crt `
  -certfile rootCA.crt `
  -passout pass:SuperSecretP@ss
