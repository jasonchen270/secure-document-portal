# Security model

## Threats considered

| Threat                                    | Mitigation                                                  |
|-------------------------------------------|-------------------------------------------------------------|
| Credential stuffing / brute force         | BCrypt password hashing (work factor 11), audit of failed logins, ready for Redis rate-limit |
| Stolen JWT replay                         | Short access-token TTL (15 min) + refresh-token rotation + hash-stored refresh tokens |
| Stolen refresh token                      | Refresh tokens stored as SHA-256 hashes; rotation invalidates the old; reuse of revoked token is detectable |
| Horizontal escalation (user A reads B's doc) | `DocumentService` filters by `OwnerId` for Uploader role; service-layer check is the source of truth (not just controller-level) |
| Tampered document content                  | SHA-256 hash stored per version; can be re-verified on download |
| SQL injection                              | EF Core parameterised queries throughout |
| XSS                                        | Angular's built-in HTML escaping; nginx sends `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY` |
| Container escape / image vulns             | Trivy scans block HIGH/CRITICAL CVEs in CI; non-root user; read-only root FS; dropped capabilities |
| Lateral movement in cluster                | Default-deny `NetworkPolicy`; explicit allows for api↔redis and api→postgres/blob only |
| Secrets in image                           | None in image; all sensitive config via K8s `Secret` (production should source from Azure Key Vault via Secrets Store CSI) |
| Audit gap                                  | Every auth event + every document mutation writes an `AuditEvent` row, viewable by Reviewer+ |

## What is intentionally not built

- **MFA**: would be the next addition. Hooks: extend `User` with a TOTP secret, add a step between password verify and token issue.
- **Document encryption at rest beyond Azure-managed**: Azure Storage Service Encryption is on by default; customer-managed keys (CMK) via Key Vault would be the next step for compliance regimes that require it.
- **Field-level encryption of metadata**: titles/classifications are plaintext in Postgres. A Restricted-classification policy would warrant pgcrypto column encryption.
- **Distributed denylist for revoked JWTs**: refresh-token revocation works, but a stolen access token is valid until its 15-min TTL. Redis denylist by `jti` is the standard upgrade.

## JWT details

- Algorithm: HS256 (symmetric). For multi-service deployments, switch to RS256 with key rotation via Key Vault.
- Claims: `sub` (user id), `email`, `role`, `jti` (unique per token), `nbf`, `exp`, `iss`, `aud`
- Clock skew allowance: 30 seconds

## Audit events captured

| Action                  | Fields                                              |
|-------------------------|-----------------------------------------------------|
| `auth.login.success`    | actor, ip                                           |
| `auth.login.fail`       | attempted email, ip (actor null if user unknown)    |
| `document.create`       | actor, document id                                  |
| `document.version.add`  | actor, version id, sha256, size                     |
| `document.download`     | actor, version id                                   |
| `document.delete`       | actor, document id                                  |
