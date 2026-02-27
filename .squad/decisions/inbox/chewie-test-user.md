# Decision: Keycloak Test User for Playwright E2E Tests

**Date:** 2025  
**Author:** Chewie (DevOps/Infra)  
**Status:** Decided

## Context

The Playwright E2E test suite requires a stable test user in Keycloak's `camplog` realm. Tests need to authenticate as a real user to exercise authenticated flows.

## Decision

Create a permanent test user with known credentials in the `camplog` Keycloak realm:

| Field | Value |
|-------|-------|
| username | `testuser` |
| email | `testuser@camplog.test` |
| password | `testpass` |
| firstName | Test |
| lastName | User |
| enabled | true |
| emailVerified | true |
| credentials | permanent (not temporary) |

**Login credential for tests:** Use email `testuser@camplog.test` (not username `testuser`) because the realm has `registrationEmailAsUsername: true`.

## Consequence

- The `camplog-web` client had `directAccessGrantsEnabled` set to `true` to support token verification and future test setup flows that use ROPC (Resource Owner Password Credentials).
- This user will be recreated on each Keycloak container restart (Aspire recreates containers on each run unless using persistent volumes). E2E test setup scripts should check/create the user idempotently.
- Admin password changes on every Aspire restart — scripts must read it from container env dynamically.
