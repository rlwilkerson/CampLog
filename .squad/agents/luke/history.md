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
