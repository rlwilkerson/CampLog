# Luke — Project History

## Core Context
**Project:** CampLog — RV trip tracking application
**Owner:** Rick Wilkerson
**Stack:** .NET 10 Web API, Entity Framework Core, Npgsql, PostgreSQL, Keycloak OIDC

**Key entities:**
- Trip: Name, Description, GPS, Start/End date (user-owned)
- Location: Name, Description, GPS, Start/End date (belongs to Trip)
- Users managed via Keycloak (external IdP)

**API responsibilities:** Trip CRUD endpoints, Location CRUD endpoints, user-scoped data access

## Learnings
<!-- Append API patterns, EF migrations, service patterns below -->
- 2026-02-27: In Aspire internal service-to-service HTTP calls, `app.UseHttpsRedirection()` in `CampLog.Api\Program.cs` can trigger 307 redirects that cause .NET `HttpClient` to drop `Authorization` on cross-scheme follow-up requests.
- 2026-02-27: For internal `http://api` traffic, prefer removing HTTPS redirection in the API while keeping authentication/authorization middleware and endpoint mappings unchanged.
- 2026-02-27: Key paths for this issue: `CampLog.Api\Program.cs`, `.squad\decisions.md`, `.squad\agents\luke\history.md`, `.squad\decisions\inbox\luke-remove-https-redirect.md`.
- 2026-02-27: Keycloak dev environment (ephemeral container, no persistent signing key storage) causes logout "Invalid IDToken" when it restarts. Solution: Intercept `OnRedirectToIdentityProviderForSignOut` in OIDC config to clear `id_token_hint` parameter before sign-out, forcing fresh auth. Files: `CampLog.Web\Program.cs`, `CampLog.AppHost\keycloak\camplog-realm.json`.
- 2026-02-27: When clearing `id_token_hint` during Keycloak logout in `OnRedirectToIdentityProviderForSignOut`, explicitly set `context.ProtocolMessage.ClientId = context.Options.ClientId` so requests with `post_logout_redirect_uri` still include required OIDC parameters. Files: `CampLog.Web\Program.cs`, `CampLog.Tests\CampLog.Tests.csproj`.
- 2026-02-27: **COMPLETED** — Logout fix implemented. Decision archived to decisions.md. See `.squad\decisions.md` entry #7.
