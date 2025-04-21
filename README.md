# PharmaTrack Local Deployment

This guide describes how to generate and trust SSL certificates for local HTTPS development, deploy backend APIs using Docker, and run the PharmaTrack WPF application.

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
├── install-pharmatrack.bat      # Launches the full installer (recommended)
├── install-pharmatrack.ps1      # Main installer script
├── generate-certs.ps1           # Certificate generator script
├── deploy.ps1                   # Trusts root cert + runs Docker Compose
├── docker-compose.yml           # Docker services configuration
├── PharmaTrack.WPF/             # WPF client
├── Gateway.API/                 # API projects...
└── etc.
```

---

## 🧪 Option A: One-click install (Recommended)

Double-click `install-pharmatrack.bat` to:

- ✅ Elevate privileges (admin required)
- ✅ Generate trusted SSL certificates
- ✅ Publish the WPF app (self-contained executable)
- ✅ Deploy Docker containers
- ✅ Create a Desktop shortcut for launching the app

Once complete, you'll find a shortcut on your Desktop to run PharmaTrack.

---

## 🛠 Option B: Manual Setup

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
**Solution**: Make sure you run `deploy.ps1` or the `.bat` installer as **Administrator**.

### Problem: `The SSL connection could not be established`
**Solution**: Ensure certificates are properly generated, trusted, and mounted into Docker containers.

### Problem: `@echo` line shows weird characters (`∩╗┐@echo`)
**Solution**: Re-save the `.bat` file using encoding: UTF-8 (without BOM) or ANSI.

---

## License

This project is open source and freely available under the [MIT License](https://opensource.org/licenses/MIT).

You are free to use, modify, distribute, and integrate this solution in commercial or non-commercial projects. Attribution is appreciated but not required.

