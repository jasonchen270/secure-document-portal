# Architecture

```
                                ┌────────────────────────┐
                                │   Angular SPA (nginx)  │
                                │   web :80              │
                                └──────────┬─────────────┘
                                           │ HTTPS (Ingress)
                                           ▼
                                ┌────────────────────────┐
                                │  .NET 8 Web API        │
                                │  api :8080             │
                                │  JWT, RBAC, audit      │
                                └─┬────────┬────────┬────┘
                                  │        │        │
                ┌─────────────────┘        │        └────────────────────┐
                ▼                          ▼                             ▼
        ┌──────────────┐          ┌──────────────┐             ┌──────────────────┐
        │ PostgreSQL   │          │  Redis       │             │ Azure Blob       │
        │ users, docs, │          │  cache /     │             │ document binaries│
        │ versions,    │          │  rate limit  │             │ SHA-256 hashed   │
        │ audit log,   │          │              │             │                  │
        │ refresh tok. │          │              │             │                  │
        └──────────────┘          └──────────────┘             └──────────────────┘
```

## Request flow: document download

1. Browser sends `GET /api/documents/{id}/download` with `Authorization: Bearer <jwt>`
2. ASP.NET JWT middleware validates signature, issuer, audience, lifetime
3. `[Authorize]` on the controller permits any authenticated principal to enter
4. `DocumentService.GetAsync` enforces row-level access (Uploader role only sees own rows)
5. Latest `DocumentVersion` is looked up, blob URI resolved
6. Blob streamed from Azure Blob Storage via `AzureBlobStorage.DownloadAsync`
7. `AuditEvent` written with action `document.download`, actor, target version id, IP
8. Response streams back to the browser

## Versioning

Each upload creates an immutable `DocumentVersion` row:

- `Version` auto-increments per document
- `Sha256` stored at upload time; lets you detect tampering and de-dupe
- `BlobUri` points to a unique path: `{documentId}/v{n}-{guid}-{filename}`
- Soft-delete on the parent `Document` hides all versions but preserves audit history

## RBAC

Roles flow as a strict hierarchy:

- **Admin** → everything, including user management
- **Reviewer** → read all documents, read audit log
- **Uploader** → CRUD only their own documents

Policies (in `Auth/Roles.cs`) wrap these into `[Authorize(Policy = ...)]` so controllers stay declarative.

## Persistence

- EF Core code-first migrations run at API startup (`db.Database.Migrate()` in `Program.cs`)
- Postgres in the demo is a local container; in AKS it's Azure Database for PostgreSQL Flexible Server (TLS required, private endpoint)

## Caching

Redis is wired up but currently unused. It is left as scaffolding for rate-limit counters per IP, a session cache for JWT denylist, and a per-user document-list cache (with invalidation on write).
