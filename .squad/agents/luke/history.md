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
