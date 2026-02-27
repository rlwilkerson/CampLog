# Chewie — Project History

## Core Context
**Project:** CampLog — RV trip tracking application
**Owner:** Rick Wilkerson
**Stack:** .NET Aspire (CLI-based), Keycloak, PostgreSQL, Docker

**Infrastructure responsibilities:**
- Aspire AppHost: wires web app, API, PostgreSQL, Keycloak as named resources
- Keycloak realm: "camplog", client: "camplog-web", OIDC redirect URIs
- PostgreSQL: connection strings via Aspire service bindings
- Environment config: dev secrets, prod-ready env vars

**Aspire note:** Using CLI (`dotnet aspire`), not Visual Studio tooling

## Learnings
<!-- Append Aspire config patterns, Keycloak setup notes, env config below -->

### Keycloak Test User Setup (2025)

**Admin credentials:** Discovered via `docker inspect keycloak-wdysamyc --format "{{range .Config.Env}}{{println .}}{{end}}"`. The Aspire Keycloak hosting integration sets `KC_BOOTSTRAP_ADMIN_USERNAME=admin` and generates a random `KC_BOOTSTRAP_ADMIN_PASSWORD` at container start. The password changes on every Aspire restart — always re-read it from container env.

**Test user created:**
- username: `testuser`, email: `testuser@camplog.test`, firstName: Test, lastName: User
- enabled: true, emailVerified: true, credentials: permanent (non-temporary)
- Keycloak user ID: `1110ee1f-487a-41d9-b8a3-232aadd88968` (may change on container recreation)

**Important: login uses email, not username.** The camplog realm has `registrationEmailAsUsername: true`, so login requires the email address (`testuser@camplog.test`) rather than the username (`testuser`). Direct access grants were enabled on the `camplog-web` client to support Playwright E2E test setup.

**How to verify Keycloak user exists:**
```powershell
# Get admin token
$token = (Invoke-RestMethod -Method Post -Uri "http://localhost:8080/realms/master/protocol/openid-connect/token" `
    -ContentType "application/x-www-form-urlencoded" `
    -Body @{ username="admin"; password="<from docker inspect>"; grant_type="password"; client_id="admin-cli" }).access_token
# Check user
Invoke-RestMethod -Uri "http://localhost:8080/admin/realms/camplog/users?username=testuser" -Headers @{ Authorization="Bearer $token" }
```
