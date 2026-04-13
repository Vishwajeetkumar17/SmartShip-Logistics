# SmartShip Platform

SmartShip is a microservices-based logistics platform with:

- Angular frontend (`smartship-ui`)
- .NET backend services (`SmartShip.Logistics`)
- API Gateway (`SmartShip.Gateway`) for unified routing and aggregated Swagger
- RabbitMQ-based event communication between services

This README is the primary local setup and run guide for the full project.

## Repository Structure

```text
SmartShip/
├─ smartship-ui/                     # Angular 21 frontend
└─ SmartShip.Logistics/              # .NET 10 backend solution
	 ├─ SmartShip.Gateway/
	 ├─ SmartShip.IdentityService/
	 ├─ SmartShip.ShipmentService/
	 ├─ SmartShip.TrackingService/
	 ├─ SmartShip.DocumentService/
	 ├─ SmartShip.AdminService/
	 ├─ SmartShip.EventBus/            # Shared RabbitMQ abstractions/infrastructure
	 ├─ SmartShip.Shared.Common/
	 └─ SmartShip.Shared.DTOs/
```

## Services Overview

| Service                   | Responsibility                                                | Docker HTTP/HTTPS | Gateway Route Prefix |
| ------------------------- | ------------------------------------------------------------- | ----------------: | -------------------- |
| SmartShip.Gateway         | Single entry point, reverse proxy, merged Swagger doc         |     `5000 / 8000` | N/A                  |
| SmartShip.IdentityService | Authentication, JWT issuance/validation                       |     `5001 / 8001` | `/identity`          |
| SmartShip.ShipmentService | Shipment + package operations, publishes events               |     `5002 / 8002` | `/shipment`          |
| SmartShip.TrackingService | Tracking lifecycle, consumes shipment events                  |     `5003 / 8003` | `/tracking`          |
| SmartShip.DocumentService | Document metadata/file operations, consumes shipment events   |     `5004 / 8004` | `/document`          |
| SmartShip.AdminService    | Admin workflows, shipment exception handling, consumes events |     `5005 / 8005` | `/admin`             |

## Gateway Endpoints

- Gateway base (HTTP): `http://localhost:5000`
- Gateway base (HTTPS): `https://localhost:8000`
- Swagger UI (HTTP): `http://localhost:5000/swagger`
- Swagger UI (HTTPS): `https://localhost:8000/swagger`
- Aggregated docs: `http://localhost:5000/swagger/gatewaydocs/{identity|shipment|tracking|document|admin}`

The gateway forwards requests to downstream services by route prefix:

- `/identity/**`
- `/shipment/**`
- `/tracking/**`
- `/document/**`
- `/admin/**`

## Frontend (Angular)

- Project: `smartship-ui`
- Angular CLI: `21.2.x`
- Dev URL: `http://localhost:4200`
- API base URL (dev): `http://localhost:5000` (Gateway)
- API base URL (prod): `https://api.smartship.com` (Gateway)

Frontend calls only the API Gateway. It does not call individual microservices directly and does not use Swagger endpoints.

## Shared Infrastructure

### SQL Server

Each service uses its own database (configured in each service `appsettings.json`):

- `SmartShipIdentityDB`
- `SmartShipShipmentDb`
- `SmartShipTrackingDb`
- `SmartShipDocumentDb`
- `SmartShipAdminDb`

### RabbitMQ

Event-driven communication is configured for Shipment, Tracking, Document, and Admin services.

Default local settings:

- Host: `localhost`
- Port: `5672`
- Username: `guest`
- Password: `guest`

## Prerequisites

Install and verify:

1. .NET SDK 10.0 (matching project target framework `net10.0`)
2. Node.js + npm (npm 10+ recommended)
3. SQL Server (local/default instance or adjust connection strings)
4. RabbitMQ running locally

Optional but recommended:

- Visual Studio 2022 / VS Code with C# Dev Kit

## Quick Start (Recommended)

Open two terminals from repository root.

### 1) Start backend services

Run each service in its own terminal (or use your IDE multi-start profile):

```powershell
dotnet run --project .\SmartShip.Logistics\SmartShip.IdentityService\SmartShip.IdentityService.csproj
dotnet run --project .\SmartShip.Logistics\SmartShip.ShipmentService\SmartShip.ShipmentService.csproj
dotnet run --project .\SmartShip.Logistics\SmartShip.TrackingService\SmartShip.TrackingService.csproj
dotnet run --project .\SmartShip.Logistics\SmartShip.DocumentService\SmartShip.DocumentService.csproj
dotnet run --project .\SmartShip.Logistics\SmartShip.AdminService\SmartShip.AdminService.csproj
dotnet run --project .\SmartShip.Logistics\SmartShip.Gateway\SmartShip.Gateway.csproj
```

### 2) Start frontend

```powershell
cd .\smartship-ui
npm install
npm start
```

Open `http://localhost:4200`.

## Build and Test

### Frontend

```powershell
cd .\smartship-ui
npm run build
npm test
```

### Backend

```powershell
dotnet build .\SmartShip.Logistics\SmartShip.Logistics.slnx
```

## Service-by-Service Notes

### SmartShip.IdentityService

- Handles auth and JWT token generation/validation.
- Applies rate limiting for sensitive auth endpoints.
- Swagger available in Development.

### SmartShip.ShipmentService

- Core shipment and package workflows.
- Publishes domain events via RabbitMQ.

### SmartShip.TrackingService

- Maintains tracking data/status updates.
- Runs background event consumer (`TrackingShipmentEventsConsumerService`).

### SmartShip.DocumentService

- Manages document records and file storage.
- Serves static files and runs background event consumer (`DocumentShipmentEventsConsumerService`).

### SmartShip.AdminService

- Provides admin-level operations and shipment exception handling.
- Integrates with Shipment service via typed `HttpClient`.
- Runs background event consumer (`AdminShipmentExceptionConsumerService`).

### SmartShip.Gateway

- Exposes a unified API entry point.
- Proxies all service routes.
- Builds an aggregated Swagger document from downstream services.

## Configuration Checklist

Before running end-to-end, confirm:

- SQL connection strings are valid for your machine.
- RabbitMQ is reachable at configured host/port.
- `JwtSettings` are present and consistent across services.
- Gateway service base URLs in `SmartShip.Gateway/appsettings.Development.json` match currently running service URLs.
- Admin service `ServiceUrls:ShipmentService` points to Shipment service URL.

## Troubleshooting

- Gateway shows 502 in merged swagger:
  - One or more downstream services are not running/reachable.
- Frontend API calls fail in development:
  - Verify gateway is running on `http://localhost:5000` for proxy target.
- Auth failures across services:
  - Ensure JWT issuer/audience/secret values match across all backend services.
- Event processing not happening:
  - Verify RabbitMQ is running and credentials/host match service settings.

## Useful URLs (Dockerized Backend)

- Frontend: `http://localhost:4200`
- Gateway Swagger UI (HTTP): `http://localhost:5000/swagger`
- Gateway Swagger UI (HTTPS): `https://localhost:8000/swagger`
- Service Swagger via Gateway:
  - `http://localhost:5000/identity/swagger/v1/swagger.json`
  - `http://localhost:5000/shipment/swagger/v1/swagger.json`
  - `http://localhost:5000/tracking/swagger/v1/swagger.json`
  - `http://localhost:5000/document/swagger/v1/swagger.json`
  - `http://localhost:5000/admin/swagger/v1/swagger.json`
- RabbitMQ Management: `http://localhost:15672`
- Seq Dashboard: `http://localhost:5341`

---

If you add a new service, update this README with:

1. Service purpose
2. Port(s)
3. Gateway prefix mapping
4. Infra dependencies
5. Startup/run instructions
