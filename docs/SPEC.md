# CampLog — Technical Stack Specification

**Version:** 1.0  
**Last Updated:** 2026-02-28  
**Lead Architect:** Yoda

---

## 1. Project Overview

**CampLog** is a web application for tracking RV trips and their associated locations. It enables authenticated users to create, manage, and organize multi-location trips with GPS coordinates, dates, and descriptions.

### Target Users
- RV travelers and enthusiasts
- Users planning or documenting road trips with multiple stops

### Core Goals
- Provide a mobile-first, responsive UI for managing trip data on-the-go
- Secure multi-user authentication via OIDC
- GPS-enabled location tracking per trip
- Flexible frontend options (Razor+HTMX, React+MUI)

---

## 2. Solution Structure

CampLog is a multi-project .NET solution orchestrated by .NET Aspire. The solution file is **CampLog.slnx**.

| Project | Type | Purpose |
|---------|------|---------|
| **CampLog.AppHost** | Aspire AppHost | Orchestrates all services (API, Web, Web2, Web3, Keycloak, Postgres, pgAdmin) |
| **CampLog.Api** | ASP.NET Core Web API | RESTful API for trips/locations, JWT auth, EF Core + PostgreSQL |
| **CampLog.Web** | Razor Pages | Primary frontend: Razor Pages + HTMX + Pico CSS (modernized design) |
| **CampLog.Web2** | Razor Pages | Clone of CampLog.Web for parallel development/experimentation |
| **CampLog.Web3** | React (Vite) | React + Material-UI (MUI) SPA with OIDC auth |
| **CampLog.ServiceDefaults** | Shared Library | Aspire service defaults (telemetry, health checks, service discovery) |
| **CampLog.Tests** | xUnit | Unit + integration + Playwright UI tests |

### Project Dependencies
- **CampLog.AppHost** references: CampLog.Api, CampLog.Web, CampLog.Web2
- **CampLog.Api** references: CampLog.ServiceDefaults
- **CampLog.Web** references: CampLog.ServiceDefaults
- **CampLog.Web2** references: CampLog.ServiceDefaults
- **CampLog.Web3**: Standalone Vite app, no .NET references (env-configured API/Keycloak endpoints)
- **CampLog.Tests** references: CampLog.Api, CampLog.Web

---

## 3. Tech Stack

### Runtime & Frameworks

| Component | Version/Details |
|-----------|----------------|
| **.NET SDK** | 10.0.103 |
| **Target Framework** | net10.0 |
| **.NET Aspire SDK** | 13.1.2 (CLI-based) |
| **ASP.NET Core** | 10.0.3 |
| **Entity Framework Core** | 10.0.3 |

### Backend (CampLog.Api)

| Package | Version |
|---------|---------|
| **Aspire.Npgsql.EntityFrameworkCore.PostgreSQL** | 13.1.2 |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | 10.0.3 |
| **Microsoft.AspNetCore.OpenApi** | 10.0.2 |
| **Microsoft.EntityFrameworkCore.Design** | 10.0.3 |
| **Npgsql.EntityFrameworkCore.PostgreSQL** | 10.0.0 |

### Frontend — CampLog.Web & CampLog.Web2 (Razor Pages)

| Package/Library | Version |
|----------------|---------|
| **Microsoft.AspNetCore.Authentication.OpenIdConnect** | 10.0.3 |
| **Pico CSS** | 2.x (CDN) |
| **HTMX** | 2.0.4 (CDN) |

### Frontend — CampLog.Web3 (React SPA)

| Package | Version |
|---------|---------|
| **React** | 19.2.0 |
| **React Router** | 7.13.1 |
| **Material-UI (MUI)** | 7.3.8 |
| **@mui/icons-material** | 7.3.8 |
| **react-oidc-context** | 3.3.0 |
| **oidc-client-ts** | 3.4.1 |
| **TanStack React Query** | 5.90.21 |
| **React Hook Form** | 7.71.2 |
| **Zod** | 4.3.6 |
| **TypeScript** | 5.9.3 |
| **Vite** | 5.4.0 |

### Infrastructure & Orchestration (Aspire AppHost)

| Component | Version/Details |
|-----------|----------------|
| **Aspire.Hosting.PostgreSQL** | 13.1.2 |
| **Aspire.Hosting.Keycloak** | 13.1.2-preview.1.26125.13 |
| **Aspire.Hosting.JavaScript** | 13.1.2 |
| **PostgreSQL** | Latest (via Aspire container) |
| **Keycloak** | Latest (via Aspire container) |
| **pgAdmin** | Latest (via Aspire container) |

### Testing (CampLog.Tests)

| Package | Version |
|---------|---------|
| **xUnit** | 2.9.3 |
| **xunit.runner.visualstudio** | 3.1.4 |
| **Microsoft.NET.Test.Sdk** | 17.14.1 |
| **Microsoft.Playwright** | 1.58.0 |
| **Microsoft.AspNetCore.Mvc.Testing** | 10.0.3 |
| **Microsoft.EntityFrameworkCore.InMemory** | 10.0.3 |
| **Moq** | 4.20.72 |
| **coverlet.collector** | 6.0.4 |

### Service Defaults (CampLog.ServiceDefaults)

| Package | Version |
|---------|---------|
| **Microsoft.Extensions.Http.Resilience** | 10.1.0 |
| **Microsoft.Extensions.ServiceDiscovery** | 10.1.0 |
| **OpenTelemetry.Exporter.OpenTelemetryProtocol** | 1.14.0 |
| **OpenTelemetry.Extensions.Hosting** | 1.14.0 |
| **OpenTelemetry.Instrumentation.AspNetCore** | 1.14.0 |
| **OpenTelemetry.Instrumentation.Http** | 1.14.0 |
| **OpenTelemetry.Instrumentation.Runtime** | 1.14.0 |

---

## 4. Architecture

### Aspire Orchestration (AppHost.cs)

CampLog uses .NET Aspire to orchestrate all services. The AppHost defines:

```csharp
var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var db = postgres.AddDatabase("camplogdb");

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithRealmImport("keycloak");

var api = builder.AddProject<Projects.CampLog_Api>("api")
    .WithReference(db).WaitFor(db)
    .WithReference(keycloak).WaitFor(keycloak);

builder.AddProject<Projects.CampLog_Web>("web")
    .WithReference(api).WaitFor(api)
    .WithReference(keycloak).WaitFor(keycloak);

builder.AddProject<Projects.CampLog_Web2>("web2")
    .WithReference(api).WaitFor(api)
    .WithReference(keycloak).WaitFor(keycloak);

builder.AddViteApp("web3", "../CampLog.Web3")
    .WithReference(api).WaitFor(api)
    .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("https"))
    .WithEnvironment("VITE_KEYCLOAK_URL", keycloak.GetEndpoint("http"))
    .PublishAsDockerFile();
```

### Service Dependencies

**Startup Order:**
1. **postgres** (PostgreSQL + pgAdmin)
2. **keycloak** (imports `camplog-realm.json`)
3. **api** (waits for db + keycloak)
4. **web, web2, web3** (wait for api + keycloak)

### API Architecture

- **Pattern:** RESTful API with JWT bearer authentication
- **Endpoints:** Grouped by resource (Trips, Locations)
- **Authorization:** All endpoints require authenticated user
- **Data Access:** Repository pattern (ITripRepository, ILocationRepository)
- **User Mapping:** IUserService auto-creates User records from Keycloak claims (sub, email, preferred_username)

### Frontend Architecture

#### CampLog.Web / Web2 (Razor + HTMX)
- **Pattern:** Server-side rendering with HTMX for interactivity
- **Layout:** Mobile-first with sticky top bar, bottom tab navigation, sheet modals
- **API Access:** HttpClient with service discovery (`https://api`)
- **Auth Flow:** OIDC code flow → Cookie-based session → API calls with token

#### CampLog.Web3 (React + MUI)
- **Pattern:** Single Page Application (SPA)
- **State Management:** TanStack React Query for API state
- **Auth Flow:** OIDC code flow via `react-oidc-context` → JWT token in memory → API calls with Bearer token
- **Build Tool:** Vite (dev server on port 3000)

---

## 5. Data Model

### Entities

#### User
| Field | Type | Notes |
|-------|------|-------|
| **Id** | Guid | PK |
| **KeycloakId** | string (256) | Unique index, maps to Keycloak sub claim |
| **Email** | string (256) | Required |
| **DisplayName** | string (256) | Required |
| **CreatedAt** | DateTimeOffset | UTC timestamp |

#### Trip
| Field | Type | Notes |
|-------|------|-------|
| **Id** | Guid | PK |
| **UserId** | Guid | FK → User, cascade delete |
| **Name** | string (256) | Required |
| **Description** | string | Nullable |
| **Latitude** | double? | Nullable GPS coordinate |
| **Longitude** | double? | Nullable GPS coordinate |
| **StartDate** | DateOnly? | Nullable |
| **EndDate** | DateOnly? | Nullable |
| **CreatedAt** | DateTimeOffset | UTC timestamp |
| **UpdatedAt** | DateTimeOffset | UTC timestamp |
| **Locations** | Collection<Location> | Navigation property |

#### Location
| Field | Type | Notes |
|-------|------|-------|
| **Id** | Guid | PK |
| **TripId** | Guid | FK → Trip, cascade delete |
| **Name** | string (256) | Required |
| **Description** | string | Nullable |
| **Latitude** | double? | Nullable GPS coordinate |
| **Longitude** | double? | Nullable GPS coordinate |
| **StartDate** | DateOnly? | Nullable |
| **EndDate** | DateOnly? | Nullable |
| **CreatedAt** | DateTimeOffset | UTC timestamp |
| **UpdatedAt** | DateTimeOffset | UTC timestamp |

### Relationships
- **User → Trips:** 1-to-many
- **Trip → Locations:** 1-to-many (cascade delete)

---

## 6. API

### Base URL
- **Development:** Resolved via Aspire service discovery (`https://api`)
- **CampLog.Web3:** Configured via `VITE_API_BASE_URL` env var

### Authentication
- **Method:** JWT Bearer token
- **Header:** `Authorization: Bearer <token>`
- **Token Source:** Keycloak OIDC token endpoint

### Endpoints

#### User
| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | `/me` | Get current authenticated user info | Required |

#### Trips
| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | `/trips` | List all trips for current user | Required |
| GET | `/trips/{id}` | Get single trip by ID | Required |
| POST | `/trips` | Create new trip | Required |
| PUT | `/trips/{id}` | Update existing trip | Required |
| DELETE | `/trips/{id}` | Delete trip (cascades to locations) | Required |

#### Locations
| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | `/trips/{tripId}/locations` | List all locations for trip | Required |
| GET | `/trips/{tripId}/locations/{id}` | Get single location | Required |
| POST | `/trips/{tripId}/locations` | Create new location in trip | Required |
| PUT | `/trips/{tripId}/locations/{id}` | Update existing location | Required |
| DELETE | `/trips/{tripId}/locations/{id}` | Delete location | Required |

### DTOs

#### TripDto
```csharp
record TripDto(Guid Id, string Name, string? Description, 
    double? Latitude, double? Longitude, DateOnly? StartDate, DateOnly? EndDate);
```

#### CreateTripRequest
```csharp
class CreateTripRequest {
    [Required] string Name;
    string? Description;
    double? Latitude, Longitude;
    DateOnly? StartDate, EndDate;
}
```

#### UpdateTripRequest
Same fields as CreateTripRequest.

#### LocationDto
```csharp
record LocationDto(Guid Id, Guid TripId, string Name, string? Description, 
    double? Latitude, double? Longitude, DateOnly? StartDate, DateOnly? EndDate);
```

#### CreateLocationRequest / UpdateLocationRequest
Same pattern as Trip requests.

### Authorization Rules
- Users can only access/modify their own trips and locations
- Ownership validated via `UserId` field on Trip entity
- Returns `403 Forbidden` if user attempts to access another user's resource

---

## 7. Frontend Variants

### 7.1 CampLog.Web (Primary Razor Pages UI)

**Status:** Active, modernized design system  
**Stack:** Razor Pages, HTMX, Pico CSS  
**Port:** Assigned by Aspire (typically `https://localhost:7xxx`)

#### Design System
- **Color Palette (Dusty Summer):**
  - Salmon: `#E3AA99`
  - Rose: `#CD9F8F`
  - Terracotta: `#DC7147`
  - Amber: `#D8A748`
  - White: `#FFFFFF`
  - Ink (text): `#4e3025`
  - Paper (background): `#fffdfb`

- **Typography:**
  - System font stack (fallback to Segoe UI, sans-serif)
  - H1: 750 weight, -0.01em letter-spacing
  - H2/H3: 700 weight
  - Body: 1.55 line-height

- **Layout:**
  - Mobile-first responsive design
  - Sticky top bar (56px, logo + user avatar/login)
  - Bottom tab navigation (Home, Trips, Profile)
  - Sheet modals for forms (slides up from bottom)
  - Max-width containers on desktop (700px breakpoint)
  - Vertical sidebar (86px) on ≥700px

- **Components:**
  - Cards: 1px border, subtle shadow, rounded corners
  - Buttons: Terracotta primary, Amber secondary
  - Forms: Focus states with primary color
  - Empty states with illustrations/icons

#### Pages
- **Auth:** `/Account/Login`, `/Account/Logout`, `/Account/Register`, `/Account/Profile`
- **Trips:** `/Trips` (list), `/Trips/Create`, `/Trips/Edit/{id}`, `/Trips/Delete/{id}`
- **Locations:** `/Trips/{tripId}/Locations` (nested under trips)
- **Public:** `/` (home), `/Privacy`, `/Error`

#### HTMX Patterns
- Sheet modals loaded via `hx-get` into `#trip-form-container`
- Form submissions via `hx-post`/`hx-put` with `hx-target` and `hx-swap`
- Progressive enhancement: works without JavaScript

---

### 7.2 CampLog.Web2 (Experimental Clone)

**Status:** Parallel development workspace  
**Stack:** Razor Pages, HTMX, Pico CSS (cloned from CampLog.Web)  
**Purpose:** Safe sandbox for UI experiments without affecting production Web  
**Current State:** Identical to CampLog.Web (awaiting Wedge-spec-driven divergence)

---

### 7.3 CampLog.Web3 (React SPA)

**Status:** Active, modern SPA alternative  
**Stack:** React 19, Material-UI 7, TypeScript, Vite  
**Port:** 3000 (dev server)

#### Architecture
- **Routing:** React Router v7 (BrowserRouter)
- **State:** TanStack React Query for server state
- **Forms:** React Hook Form + Zod validation
- **Auth:** `react-oidc-context` wrapper around `oidc-client-ts`
- **Theme:** Custom MUI theme matching Dusty Summer palette

#### Theme Configuration
```typescript
{
  palette: {
    primary: { main: '#DC7147' },     // terracotta
    secondary: { main: '#D8A748' },   // amber
    background: { default: '#faf7f4', paper: '#fffdfb' },
    text: { primary: '#4e3025' }
  },
  typography: { fontFamily: 'Inter, Segoe UI, system-ui, sans-serif' },
  shape: { borderRadius: 12 }
}
```

#### Pages
- `/callback`: OIDC callback handler
- `/trips`: Trips list (default route)
- `/trips/:tripId`: Trip detail with locations

#### API Client
- `src/api/client.ts`: Typed fetch wrapper
- Token management via module-level setter pattern
- Auto-injects `Authorization: Bearer <token>` header

#### Build & Dev
- **Dev Server:** `npm run dev` (port 3000)
- **Build:** `npm run build` (TypeScript + Vite)
- **Aspire Integration:** Published as Docker container with env var config

---

## 8. Identity & Authentication

### Keycloak Configuration

**Realm:** `camplog`  
**Port:** 8080 (HTTP)  
**Admin Console:** `http://localhost:8080`

#### Realm Settings
- **Registration:** Enabled
- **Email as Username:** Enabled
- **Realm Roles:** `user`, `admin`

#### Test User (Imported from `camplog-realm.json`)
- **Username:** `testuser`
- **Email:** `testuser@camplog.test`
- **Password:** `testpass`
- **Name:** Test User
- **Roles:** `user`
- **Email Verified:** Yes

#### Client: `camplog-web`
- **Protocol:** openid-connect
- **Type:** Public client (no client secret for browser flows)
- **Flows Enabled:**
  - Standard Flow (Authorization Code)
  - Direct Access Grants
- **Redirect URIs:** `*` (wildcard for dev; restrict in production)
- **Web Origins:** `*` (CORS, wildcard for dev)
- **Post-Logout Redirect URIs:** `+` (allow all valid redirects)
- **Protocol Mapper:**
  - **Name:** `camplog-web-audience`
  - **Type:** oidc-audience-mapper
  - **Config:** Includes `camplog-web` in access token audience

#### Realm Import
- **File:** `CampLog.AppHost/keycloak/camplog-realm.json`
- **Loaded by Aspire:** `.WithRealmImport("keycloak")`

---

### OIDC Flow

#### CampLog.Web / Web2 (Razor Pages)
1. User navigates to protected page
2. Middleware redirects to Keycloak login (`/realms/camplog/protocol/openid-connect/auth`)
3. User authenticates with Keycloak
4. Keycloak redirects to `/signin-oidc` with authorization code
5. Middleware exchanges code for tokens, establishes cookie session
6. Subsequent requests use cookie; API calls include JWT from saved token

**Cookie Settings:**
- `SameSite=Lax` (development workaround for mixed HTTP/HTTPS)
- Production should use `SameSite=None; Secure=true` with full HTTPS

**Event Handlers:**
- `OnRedirectToIdentityProviderForSignOut`: Clears `id_token_hint` to avoid Keycloak restart errors in dev

#### CampLog.Web3 (React SPA)
1. App loads, checks auth state
2. If not authenticated, redirects to Keycloak
3. User authenticates
4. Keycloak redirects to `/callback` with authorization code
5. `react-oidc-context` exchanges code for tokens, stores in memory
6. App navigates to `/trips`, all API calls include `Authorization: Bearer <token>`

**Token Storage:** In-memory only (no localStorage for security)

---

### API Authentication

**Scheme:** JWT Bearer  
**Authority:** `http://localhost:8080/realms/camplog`  
**Audience:** `camplog-web`  
**HTTPS Metadata:** Disabled (development only)

**Token Validation:**
- Validates issuer, audience, signature
- Maps `preferred_username` to `NameClaimType`
- Maps `realm_access.roles` to `RoleClaimType`

**User Resolution (ClaimsPrincipalExtensions.cs):**
```csharp
GetKeycloakId(): user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
GetEmail(): user.FindFirstValue("email")
GetDisplayName(): user.FindFirstValue("preferred_username")
```

**User Auto-Creation:**
- IUserService.GetOrCreateAsync() checks for existing User by KeycloakId
- If not found, creates new User record in database
- Enables just-in-time user provisioning

---

## 9. Infrastructure

### PostgreSQL
- **Service Name:** `postgres`
- **Database:** `camplogdb`
- **Port:** Assigned by Aspire (typically 5432)
- **Management:** pgAdmin included via `.WithPgAdmin()`
- **Connection String:** Auto-configured via Aspire service discovery

### Keycloak
- **Service Name:** `keycloak`
- **Port:** 8080
- **Realm Import:** `CampLog.AppHost/keycloak/camplog-realm.json`
- **Container:** Latest Keycloak image from Aspire hosting

### Aspire Dashboard
- **URL:** `http://localhost:15888` (or assigned by Aspire)
- **Features:**
  - Resource status monitoring
  - Logs (console + structured)
  - Traces (OpenTelemetry)
  - Metrics
  - Environment variables
  - Endpoints

### OpenTelemetry (Service Defaults)
- **Exporters:** OTLP (if `OTEL_EXPORTER_OTLP_ENDPOINT` configured)
- **Instrumentation:**
  - ASP.NET Core (HTTP requests, excludes `/health`, `/alive`)
  - HTTP Client
  - Runtime metrics
- **Logs:** Includes formatted messages and scopes
- **Traces:** Auto-tagged with service name

### Health Checks
- **Endpoint:** `/health` (all checks)
- **Liveness Endpoint:** `/alive` (tagged with `live`)
- **Configured by:** CampLog.ServiceDefaults
- **Environment:** Development only (security consideration)

---

## 10. Development Setup

### Prerequisites
1. **.NET 10 SDK** (10.0.103 or later)
2. **Docker Desktop** (for Postgres, Keycloak, pgAdmin containers)
3. **Node.js 20+** (for CampLog.Web3 only)
4. **Aspire CLI** (installed via .NET SDK)

### Running the Application

```bash
cd C:\code\rlwilkerson\CampLog\main
aspire run
```

This starts:
- PostgreSQL (with pgAdmin)
- Keycloak (imports camplog realm)
- CampLog.Api
- CampLog.Web
- CampLog.Web2
- CampLog.Web3 (npm install + npm run dev)

**Aspire Dashboard:** Auto-opens in browser (`http://localhost:15888`)

### Environment Variables (CampLog.Web3)
Create `.env` in `CampLog.Web3`:
```
VITE_API_BASE_URL=https://localhost:<api-port>
VITE_KEYCLOAK_URL=http://localhost:8080
VITE_KEYCLOAK_CLIENT_ID=camplog-web
```
(Aspire auto-injects these when orchestrating)

### Test Credentials
- **Username:** `testuser@camplog.test` or `testuser`
- **Password:** `testpass`

### Ports (Typical Development)
| Service | Port | Protocol |
|---------|------|----------|
| Keycloak | 8080 | HTTP |
| PostgreSQL | 5432 | TCP |
| pgAdmin | Varies | HTTP |
| CampLog.Api | 7xxx | HTTPS |
| CampLog.Web | 7xxx | HTTPS |
| CampLog.Web2 | 7xxx | HTTPS |
| CampLog.Web3 | 3000 | HTTP |
| Aspire Dashboard | 15888 | HTTP |

**Note:** Exact HTTPS ports assigned dynamically by Aspire (check dashboard).

### Database Migrations

**Create Migration:**
```bash
cd CampLog.Api
dotnet ef migrations add <MigrationName>
```

**Apply Migration:**
```bash
dotnet ef database update
```

**Connection String:** Auto-configured via Aspire; check `appsettings.json` or environment variables for manual connections.

---

## 11. Key Decisions

### Architecture & Design

1. **Aspire CLI over Workload (2025-02-28)**
   - Aspire workload is obsolete; use Aspire CLI exclusively
   - Benefits: Simpler updates, no global SDK pollution

2. **Mobile-First Design Requirement (2025-01-01)**
   - All UI must be responsive and mobile-optimized
   - Bottom tab navigation for mobile, sidebar for desktop
   - Touch targets ≥44px, 4.5:1 contrast ratio (WCAG AA)

3. **Color Theme — Dusty Summer (2025-01-01)**
   - Palette locked: #E3AA99, #CD9F8F, #DC7147, #D8A748, #FFFFFF
   - Consistent across Web, Web2, Web3 frontends

4. **Multiple Frontend Strategy (2026-02-28)**
   - **CampLog.Web:** Production Razor+HTMX UI (modernized)
   - **CampLog.Web2:** Experimental clone for parallel development
   - **CampLog.Web3:** React+MUI SPA for modern SPA use case

### Authentication & Security

5. **OIDC Login Redirect Fix (2027-01-27)**
   - `SameSite=Lax` for development (mixed HTTP/HTTPS flows)
   - Production requires stricter `SameSite=None; Secure=true`

6. **Preserve Local Login Return URLs (2026-02-27)**
   - All login links include `returnUrl` query parameter
   - Only local app routes accepted (security)

7. **Sub Claim Resolution in API (2026-02-27)**
   - API resolves user ID from `sub` (raw) or `ClaimTypes.NameIdentifier` (mapped)
   - Handles both Keycloak token formats and ASP.NET claim mapping

### Data & Persistence

8. **Avoid Persistent Containers Early (Aspire Guidance)**
   - Don't persist containers during active development
   - Reduces state management issues when restarting app
   - Use persistent volumes only when data stability required

### Testing & Quality

9. **Playwright Infra Fixture as Default (2026-02-28)**
   - `AspireAppHostFixture.cs` provides:
     - Auto-start of Aspire AppHost
     - Keycloak readiness checks
     - Test user token acquisition
     - Env-overridable base URL (`CAMPLOG_TEST_BASE_URL`)
   - Default: `CAMPLOG_TEST_AUTOSTART_APPHOST=true`

10. **Web2 Restart Blocker Ownership Split (2026-02-28)**
    - OIDC callback/infra → Chewie (DevOps)
    - Server-side exceptions → Luke (Backend)
    - UI/acceptance regressions → Leia (Frontend)
    - Explicit ownership prevents cross-team ambiguity

### Frontend Modernization

11. **Frontend Modernization Design System (2025-02-28)**
    - Extend Pico CSS with custom `camplog.css`
    - Typography hierarchy, spacing system, component conventions
    - 5-phase rollout: tokens → layout → content → forms → edge cases
    - Preserve Dusty Summer palette, HTMX flows, mobile-first layout

---

## 12. Appendix

### File Locations

| Path | Description |
|------|-------------|
| `CampLog.AppHost/AppHost.cs` | Aspire orchestration config |
| `CampLog.AppHost/keycloak/camplog-realm.json` | Keycloak realm import |
| `CampLog.Api/Program.cs` | API startup, JWT config, endpoints |
| `CampLog.Api/Data/CampLogDbContext.cs` | EF Core context |
| `CampLog.Api/Models/` | Entity classes (User, Trip, Location) |
| `CampLog.Api/Endpoints/` | Minimal API endpoint groups |
| `CampLog.Web/Pages/` | Razor Pages |
| `CampLog.Web/wwwroot/css/camplog.css` | Custom CSS design system |
| `CampLog.Web3/src/App.tsx` | React app routing |
| `CampLog.Web3/src/api/client.ts` | API client |
| `CampLog.Web3/src/theme.ts` | MUI theme (Dusty Summer) |
| `CampLog.ServiceDefaults/Extensions.cs` | Aspire service defaults |
| `global.json` | .NET SDK version pinning |
| `CampLog.slnx` | Solution file |

### Commands Reference

| Task | Command |
|------|---------|
| Run application | `aspire run` |
| Run tests | `dotnet test` |
| Run Playwright tests | `dotnet test --filter "Category=PlaywrightUI"` |
| Build solution | `dotnet build` |
| Restore packages | `dotnet restore` |
| Create migration | `dotnet ef migrations add <Name> -p CampLog.Api` |
| Update database | `dotnet ef database update -p CampLog.Api` |
| Install Web3 deps | `cd CampLog.Web3 && npm install` |
| Run Web3 dev server | `cd CampLog.Web3 && npm run dev` |
| Build Web3 | `cd CampLog.Web3 && npm run build` |

### Team Roster

| Role | Agent | Domain |
|------|-------|--------|
| **Lead / Architect** | Yoda | Technical vision, architecture, decomposition |
| **Backend** | Luke | .NET API, EF Core, data layer, server-side logic |
| **Frontend** | Leia | Razor Pages, HTMX, CSS, React, UI/UX |
| **DevOps / Infra** | Chewie | Aspire, Docker, Keycloak, CI/CD, infrastructure |
| **QA** | Wedge | Tests, Playwright, acceptance criteria, quality gates |

---

**End of Specification**
