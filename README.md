# Secure Document Portal

A document management portal with role-based access, audit logging, and versioned uploads.

- **Frontend:** Angular 17 (standalone components, signals, function-style guards/interceptors)
- **Backend:** .NET 8 Web API with JWT (access + refresh-token rotation), policy-based RBAC, EF Core, BCrypt password hashing
- **Storage:** PostgreSQL (metadata + audit), Azure Blob Storage (document binaries, SHA-256-hashed)
- **Cache:** Redis

## Quick start

### Lightweight mode (no Docker needed)

Requires **.NET 10 SDK** and **Node 20+** locally.

```bash
make demo-local
```

Brings up the API (SQLite + local-filesystem blob store) on `:5080` and the Angular dev server on `:4300`, then opens your browser. The same code paths run as in production. Only the EF Core provider and `IBlobStorage` implementation differ, switched by config.

### Full stack (docker-compose)

Requires **Docker Desktop** running.

```bash
make demo
```

That builds the API and web images, brings up Postgres + Redis + Azurite (Azure Blob emulator) + API + web, waits for health checks, prints credentials, and opens the app in your browser.

- Web:  http://localhost:8080
- API:  http://localhost:5080 (OpenAPI doc at `/openapi/v1.json`)
