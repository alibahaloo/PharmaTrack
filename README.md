# PharmaTrack Local Deployment

This guide walks you through installing and running PharmaTrack locally on Windows. It uses three PowerShell scripts to generate certificates, deploy services in Docker, publish the WPF client, and create a Desktop shortcut for easy launch.

---

## Prerequisites

- **Administrator Privileges**  
  Run all scripts in an elevated PowerShell session (Run as Administrator).

- **PowerShell 7+**  
  Ensure script execution is allowed:
  ```powershell
  Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
  ```

- **Docker CLI / Docker Desktop**  
  Install and running. Verify with:
  ```powershell
  docker version
  ```

- **.NET SDK 6+**  
  Used to publish the WPF application. Verify with:
  ```powershell
  dotnet --list-sdks
  ```

- **OpenSSL**  
  Required by `generate-certs.ps1`. Install via Chocolatey:
  ```powershell
  choco install openssl.light
  ```

---

## Folder Structure

```
PharmaTrack/
├── install-pharmatrack.ps1      # Main installer script (orchestrates all steps)
├── generate-certs.ps1           # Generates Root CA and server certificates
├── deploy.ps1                   # Imports Root CA and starts Docker services
├── docker-compose.yml           # Docker Compose configuration
├── PharmaTrack.WPF/             # WPF client source & project
├── Gateway.API/                 # API gateway and backend services
└── ...                          # Other solution files
```

---

## Installation

1. **Open PowerShell as Administrator**  
   ```powershell
   Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
   cd path\to\PharmaTrack
   ```

2. **Run the installer script**  
   ```powershell
   .\install-pharmatrack.ps1
   ```

This single script will:

- Check for Administrator rights  
- Verify Docker CLI is installed  
- Publish the WPF app (self-contained executable)  
- Generate and trust SSL certificates (`generate-certs.ps1`)  
- Deploy Docker containers (`deploy.ps1`)  
- Create a Desktop shortcut for PharmaTrack  

---

## Alternative Manual Steps

If you need more control, run the scripts individually:

1. **Generate certificates**  
   ```powershell
   .\generate-certs.ps1
   ```

2. **Import certificates & deploy Docker**  
   ```powershell
   .\deploy.ps1
   ```

3. **Publish WPF app & create shortcut**  
   (already part of `install-pharmatrack.ps1`; run separately only if needed)

---

## Troubleshooting

### Not running as Administrator

```
This script must be run as Administrator.
```
**Fix:** Right-click PowerShell → Run as Administrator.

### Docker CLI not found

```
Docker CLI not found. Please install Docker Desktop or Docker CLI before running this script.
```
**Fix:** Install Docker Desktop and restart PowerShell.

### SSL connection errors

**Fix:**  
- Ensure the root CA (`rootCA.crt`) is in Trusted Root Certification Authorities.  
- Delete existing certs in `%USERPROFILE%\.aspnet\https` and rerun `generate-certs.ps1`.

### Docker services fail to start

**Fix:**  
- Run `docker-compose down` then `docker-compose up` manually to inspect logs.  
- Ensure required ports (e.g., 8082, 5432) are free.

### Desktop shortcut issues

**Fix:**  
- Verify `publish\PharmaTrack.WPF.exe` exists.  
- Recreate the shortcut manually if needed.

---

## License

MIT License.