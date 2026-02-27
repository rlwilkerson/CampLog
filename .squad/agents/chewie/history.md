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

### Keycloak Realm Persistence & Audience Mapper (2025-01-23)

**Problem:** Two Keycloak configuration issues:
1. **Realm import skipped on restart**: `.WithDataVolume()` caused Keycloak to persist realm data, so changes to `camplog-realm.json` were ignored on subsequent starts (logs showed "Realm 'camplog' already exists. Import skipped").
2. **Missing audience claim**: API validates `Audience = "camplog-web"` in JWT tokens, but Keycloak 26.x does NOT include the requesting client's ID in the `aud` claim by default — only `["account"]`. This caused 401 failures even with valid Bearer tokens.

**Solution:**
1. **Removed `.WithDataVolume()`** from Keycloak resource in `AppHost.cs` → now uses ephemeral storage, realm JSON is always re-imported fresh on startup.
2. **Added audience protocol mapper** to `camplog-web` client in `camplog-realm.json`:
   ```json
   "protocolMappers": [
     {
       "name": "camplog-web-audience",
       "protocol": "openid-connect",
       "protocolMapper": "oidc-audience-mapper",
       "consentRequired": false,
       "config": {
         "included.client.audience": "camplog-web",
         "id.token.claim": "false",
         "access.token.claim": "true"
       }
     }
   ]
   ```

**Key learnings:**
- **Aspire Keycloak**: Use `.WithDataVolume()` only in production-like scenarios where realm state must persist. For local dev with declarative realm JSON, prefer ephemeral storage for reproducibility.
- **Keycloak audience**: Default OIDC access tokens do NOT include the client ID in `aud` — you must explicitly add an `oidc-audience-mapper` protocol mapper to inject the correct audience value for JWT validation.
- **File locations:**
  - AppHost: `CampLog.AppHost\AppHost.cs`
  - Realm JSON: `CampLog.AppHost\keycloak\camplog-realm.json`

### OIDC Correlation Cookie Fix (2025-01-27)

**Problem:** Auth redirect chain failed with "Correlation failed" error. Users logged out → clicked protected page (e.g., /Trips) → redirected to Keycloak login → successful login → redirect back to `/signin-oidc` → **correlation cookie missing → authentication fails**.

**Root cause:** ASP.NET Core OIDC middleware's correlation and nonce cookies default to `SameSite=None; Secure=true`, which requires HTTPS for both the app AND the identity provider. In dev, Keycloak runs on HTTP (`http://localhost:8080`) while the web app uses HTTPS → browsers reject the cookies as insecure on cross-site redirect.

**Solution:** Configure OIDC and auth cookies to use `SameSite=Lax` for dev environments in `CampLog.Web\Program.cs`:

```csharp
builder.Services.AddAuthentication(options => { ... })
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
})
.AddOpenIdConnect(options =>
{
    // ... existing config ...
    options.NonceCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    // ...
});
```

**Impact:** `SameSite=Lax` allows cookies to be sent on top-level navigation (e.g., OIDC redirects) even with mixed HTTP/HTTPS, fixing the correlation failure. For production with full HTTPS, consider using environment-based configuration to enforce stricter settings.

**Validation:** After changes, tested flow: logout → navigate to protected /Trips → redirect to Keycloak → login → successfully redirect back to /Trips with auth cookie preserved.

### OIDC & Frontend Return URL Fix (2026-02-26)

**Collaboration:** Chewie + Leia joint fix for complete login redirect chain
- **Chewie (OIDC):** Fixed `CampLog.Web\Program.cs` cookies to use `SameSite=Lax`
- **Leia (Frontend):** Propagated `returnUrl` through login entry points to ensure post-login navigation

**Cross-agent dependency:** Chewie's OIDC fix enables Leia's returnUrl enhancement to work end-to-end. Together they solve the complete redirect problem: auth succeeds (Chewie) + user returns to intended page (Leia).

**Session log:** `.squad\log\20260226-220105-login-redirect-fix.md`
