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
