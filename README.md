# About PharmaTrack

PharmaTrack is a free, modern solution designed to empower small and medium-sized pharmacies with the tools they need to operate efficiently and safely. It offers:

* **User Management**
  Secure handling of user accounts, roles, and permissions.

* **Task Management**
  Create and assign tasks to pharmacy staff with calendar views for daily, weekly, and monthly planning.

* **Inventory Management**
  Track product inventory with full support for stock-in/stock-out using UPC barcode scanners. All transactions are logged for audit and history.

* **Drug Interactions**
  Check for interactions between up to 10 drugs simultaneously. Search by product (e.g., Advil) or active ingredient (e.g., Ibuprofen).

* **Drug & Ingredient Lookup**
  Access rich details about drugs and ingredients including brand names, market availability, and product details.

**Data Sources:**

* Drug Product data: [Health Canada Drug Database](https://www.canada.ca/en/health-canada/services/drugs-health-products/drug-products/drug-product-database.html)
* Drug Interactions: [DDInter Database](https://ddinter.scbdd.com/)

---

# PharmaTrack Deployment Guide

This guide walks you through running and deploying PharmaTrack on Windows. Development is cross-platform using Docker, while production deployment is Windows-only—services are installed as Windows Services and configured to start automatically with the operating system.

## Prerequisites

* **Administrator Privileges**
  Required when running deployment scripts that install services.

* **PowerShell 7+**
  Run scripts with execution enabled:

  ```powershell
  Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
  ```

* **Docker CLI / Docker Desktop**
  Needed for containerized deployment:

  ```powershell
  docker version
  ```

* **.NET SDK 6+**
  Required to build and publish the services:

  ```powershell
  dotnet --list-sdks
  ```

---

## Folder Structure

```
PharmaTrack/
├── .github/                     # GitHub workflows and metadata
├── Auth.API/                   # Auth API service
├── Drug.API/                   # Drug API service
├── Inventory.API/              # Inventory API service
├── PharmaTrack.Core/           # Core DB Models and DTOs
├── PharmaTrack.Host/           # Host to serve static files for PWA
├── PharmaTrack.PWA/            # Progressive Web App frontend
├── PharmaTrack.Shared/         # Shared services and configurations between APIs
├── PharmaTrack.WPF/            # WPF Client
├── Schedule.API/               # Schedule API service
├── compose.yaml                # Docker Compose configuration
├── deploy.ps1                  # Publishes and installs services as Windows Services
├── PharmaTrack.sln             # Visual Studio solution file
├── README.md                   # This file
└── publish/                    # Output folder for published builds via deployment script
```

---

## Local Development

For local development, you have two supported workflows:

### Option 1: API + PWA (Docker + Visual Studio)

1. Start backend services via Docker Compose:

   ```powershell
   cd path\to\PharmaTrack
   dotnet build
   docker compose up --build -d
   ```

2. Open the solution in Visual Studio, set `PharmaTrack.PWA` as the startup project, and run/debug the app.

### Option 2: Full Visual Studio (No Docker)

1. Open the solution in Visual Studio
2. Start any API project individually (e.g., `Auth.API`, `Drug.API`, etc.)
3. Run and debug the PWA or other client apps as needed

> **Note:** Running services as Windows Services is **not intended** for local development. Use Docker or Visual Studio instead.

---

## Production Deployment

To perform a full production-like deployment on a local machine, use the interactive PowerShell script `deploy.ps1`. This script provides a guided setup with dependency checks, service publishing, and initial data seeding.

### Required Setup

1. **Open PowerShell as Administrator**

   ```powershell
   Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
   cd path\to\PharmaTrack
   ```

### Steps Performed by the Script:

* Ensures dependencies:

  * .NET 9 SDK is installed (via `winget` if missing)
  * SQL Server Express is installed and running
  * `dotnet-ef` CLI tool is installed

* Publishes and deploys services:

  * Builds each backend API as a self-contained Windows executable
  * Installs/removes Windows Services automatically with friendly names and descriptions
  * Publishes the PWA and copies it into the Host's `wwwroot`

* Optional Prompts:

  * Run EF Core database migrations
  * Deploy WPF client and create desktop shortcut
  * Import initial drug/interaction data via API endpoints
  * Register a default admin user

### To Use:

```powershell
cd path\to\PharmaTrack
.\deploy.ps1
```

> **Note:** This must be run as Administrator.

Follow the on-screen prompts to complete setup.

---

## Configuration

### Ports

**Development Environment:**

* PWA: `http://localhost:8080`
* Auth API: `http://localhost:8081`
* Drug API: `http://localhost:8082`
* Inventory API: `http://localhost:8083`
* Schedule API: `http://localhost:8084`

**Production Environment:**

* PWA: `http://localhost:9090`
* Auth API: `http://localhost:9091`
* Drug API: `http://localhost:9092`
* Inventory API: `http://localhost:9093`
* Schedule API: `http://localhost:9094`

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

### Docker issues

**Symptom:** Services fail to start or port conflicts occur
**Fix:**

* Run `docker compose down` to stop containers
* Check for port conflicts on ports like 8082, 5432, etc.

### Service fails to install or start

**Fix:**

* Ensure you're running PowerShell as Administrator
* Check `Event Viewer > Windows Logs > Application` for errors
* Check the status of services `services.msc`
* Use `sc delete <ServiceName>` to remove a service manually if needed


---

## License

This project, **PharmaTrack**, is licensed under the [Apache License 2.0](LICENSE).

You are free to use, modify, and distribute this software, provided that you:

- Include proper attribution to the original author.
- Retain the license and notice files in any redistribution.

See the [NOTICE](NOTICE) file for attribution details.

---

© 2025 Ali Bahaloo

