# Secure Document Portal

A document management portal with role-based access, audit logging, and versioned uploads. The frontend is Angular 17 (standalone components, signals, function-style guards/interceptors) and the backend is a .NET 8 Web API with JWT access plus refresh-token rotation, policy-based RBAC, EF Core, and BCrypt password hashing. Metadata and audit records live in PostgreSQL, document binaries are stored SHA-256-hashed in Azure Blob Storage, and Redis provides caching.

## Prerequisites

- .NET 10 SDK
- Node 20+
- Docker Desktop (full-stack mode only)

## Installation

Clone the repository and ensure the prerequisites above are installed. Both run modes are driven by `make`; no further setup is required.

## Usage

### Lightweight mode (no Docker)

```bash
make demo-local
```

Brings up the API (SQLite + local-filesystem blob store) on `:5080` and the Angular dev server on `:4300`, then opens your browser. The same code paths run as in production; only the EF Core provider and `IBlobStorage` implementation differ, switched by config.

### Full stack (docker-compose)

```bash
make demo
```

Builds the API and web images, brings up Postgres + Redis + Azurite (Azure Blob emulator) + API + web, waits for health checks, prints credentials, and opens the app.

- Web: http://localhost:8080
- API: http://localhost:5080 (OpenAPI doc at `/openapi/v1.json`)
