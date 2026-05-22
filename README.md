# Secure Document Portal

A document management portal with role-based access, audit logging, versioned uploads, and a production-style deployment story.

- **Frontend:** Angular 17 (standalone components, signals, function-style guards/interceptors)
- **Backend:** .NET 8 Web API with JWT (access + refresh-token rotation), policy-based RBAC, EF Core, BCrypt password hashing
- **Storage:** PostgreSQL (metadata + audit), Azure Blob Storage (document binaries, SHA-256-hashed)
- **Cache:** Redis
- **Deploy:** Docker images, Kubernetes manifests for AKS (HPA, NetworkPolicy, NonRoot/ReadOnlyRootFS), GitLab CI with Trivy image scanning and staged rollout (auto-staging, manual prod gate)

## Quick start (demo)

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

### Seeded accounts (password: `ChangeMe!123`)

| Email                    | Role     | Can do                                              |
|--------------------------|----------|-----------------------------------------------------|
| admin@portal.local       | Admin    | Everything: manage users, view audit, all docs      |
| reviewer@portal.local    | Reviewer | View all documents, view audit log                  |
| uploader@portal.local    | Uploader | Create/upload/download/delete only their own docs   |

## Layout

```
api/      .NET 8 Web API (auth, documents, users, audit)
web/      Angular SPA
k8s/      Kubernetes manifests for AKS
docs/     Architecture + security notes
.gitlab-ci.yml   build → test → Trivy scan → publish → staged rollout
docker-compose.yml   one-command local demo
```

## Useful targets

```
make demo         build + up + open browser
make demo-down    stop containers (keep data)
make demo-logs    follow logs
make clean        stop + remove volumes
```

## Security model

See [docs/SECURITY.md](docs/SECURITY.md) for the threat model and a breakdown of the controls (JWT, RBAC, audit, blob hashing, container hardening, network policy).
