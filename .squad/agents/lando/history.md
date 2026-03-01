# Lando — Project History

## Core Context
**Project:** CampLog — RV trip tracking application
**Owner:** Rick Wilkerson
**Stack:** React, TypeScript, .NET 10 API, .NET Aspire

**Color palette (Dusty Summer):**
- #E3AA99 (warm blush)
- #CD9F8F (muted rose)
- #DC7147 (terracotta/orange) — primary actions
- #D8A748 (golden amber) — accents
- #FFFFFF (white) — surfaces

**Target project:** `CampLog.Web2` — a parallel frontend implementation alongside the existing Razor Pages app (`CampLog.Web`)

**API base URL:** `https://api` (resolved via .NET Aspire service discovery)
**Auth:** Keycloak OIDC — access tokens must be forwarded as `Authorization: Bearer` on API calls

**Core API endpoints:**
- `GET /trips` — list all trips
- `POST /trips` — create trip (`{ title, startDate?, endDate?, description? }`)
- `GET /trips/{id}` — get trip detail
- `PUT /trips/{id}` — update trip
- `DELETE /trips/{id}` — delete trip
- `GET /trips/{tripId}/locations` — list locations for a trip
- `POST /trips/{tripId}/locations` — create location
- `GET /trips/{tripId}/locations/{id}` — get location
- `PUT /trips/{tripId}/locations/{id}` — update location
- `DELETE /trips/{tripId}/locations/{id}` — delete location

**Domain models:**
- Trip: `{ id, title, startDate?, endDate?, description? }`
- Location: `{ id, tripId, name, arrivalDate?, departureDate?, notes? }`

## Learnings
<!-- Append React patterns, component decisions, design choices below -->
- CampLog.Web3 scaffolded at CampLog.Web3/ using Vite + React + TypeScript + Material UI
- Dusty Summer MUI theme created in src/theme.ts
- OIDC auth via react-oidc-context + oidc-client-ts
- TanStack Query for server state, React Hook Form + Zod for forms
- AppShell with MUI AppBar + BottomNavigation (mobile) + Drawer sidebar (desktop)
- Pages: TripsPage, TripDetailPage, CallbackPage
- API client in src/api/client.ts uses module-level token store
- Env vars: VITE_API_BASE_URL, VITE_KEYCLOAK_URL, VITE_KEYCLOAK_CLIENT_ID
- Dev server runs on port 3000 (strictPort: true for Aspire compatibility)
- Web3 decision merged to decisions.md (2026-02-28)
