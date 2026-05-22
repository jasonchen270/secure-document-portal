# Incident IR-2026-0142

**Severity:** P2
**Detected:** 2026-04-18 03:41 UTC
**Resolved:** 2026-04-18 05:12 UTC

## Summary
Brief elevated 5xx rate on the auth path after Postgres failover.
Reverted to previous primary, latency normalized within 4 minutes.

## Action items
1. Tune pgbouncer reconnect backoff
2. Add alert on auth p99 > 500ms
