# PharmaTrack Local Deployment

This guide describes how to generate and trust SSL certificates for local HTTPS development, and how to launch the PharmaTrack API stack using Docker Compose.

---

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- PowerShell (Windows 10/11)
- OpenSSL installed (included in Git Bash or install via Chocolatey: `choco install openssl.light`)

---

## Folder Structure

Your solution root should contain the following:

```
PharmaTrack/
├── generate-certs.ps1        # Certificate generator script
├── deploy.ps1                # Trusts root cert + runs Docker Compose
├── docker-compose.yml        # Docker services configuration
├── PharmaTrack.sln           # Solution file
├── Gateway.API/              # API projects...
└── etc.
```

---

## Step-by-Step Deployment

### 1. Generate Local Certificates

Open PowerShell as Administrator and run:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
./generate-certs.ps1
```

This will generate:
- A root certificate authority (CA)
- A signed server certificate for `localhost`
- A `.pfx` certificate used by Kestrel in Docker


### 2. Trust the Root Certificate + Launch Docker

In the same admin PowerShell window:

```powershell
./deploy.ps1
```

This will:
- Import the root certificate into Windows trusted root store
- Start the Docker containers


### 3. Access the API Gateway

After successful launch, access your API Gateway at:

```
https://localhost:8082
```

The WPF client should be configured to point to this URL.

---

## What to Add to .gitignore

The following files are **auto-generated** and should **not be committed** to Git:

```gitignore
# SSL certificate files
aspnetapp.pfx
rootCA.crt
rootCA.key
rootCA.srl
server.key
server.crt
server.csr
openssl-san.cnf
```

---

## Troubleshooting

### Problem: Access Denied on `Import-Certificate`
**Solution**: Make sure you run `deploy.ps1` from **an Administrator PowerShell window**.

### Problem: `The SSL connection could not be established`
**Solution**: Ensure the certificate is correctly generated, trusted, and mounted in Docker.

---

## License
Internal PharmaTrack deployment tooling. Not for public distribution.