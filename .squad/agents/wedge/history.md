# Wedge — Project History

## Core Context
**Project:** CampLog — RV trip tracking application
**Owner:** Rick Wilkerson
**Stack:** xUnit, ASP.NET Core integration testing, Testcontainers (PostgreSQL)

**Testing responsibilities:**
- Unit tests: service layer, data model validation
- Integration tests: API endpoints with real PostgreSQL via Testcontainers
- Edge cases: auth failures, empty trip lists, null GPS, concurrent edits, multi-user isolation
- Quality gate: no merge without test coverage for new features

## Learnings
<!-- Append test patterns, edge cases found, quality gate rules below -->

### 2026-02-28: Frontend redesign QA gates added (design-first criteria)
- Added Playwright acceptance suite `FrontendRedesignAcceptanceTests` to enforce modern layout quality without palette drift.
- New gates now explicitly cover: Dusty Summer token preservation, readable typography thresholds, mobile 44x44 touch targets, desktop nav rail behavior, trip card spacing hierarchy, and no horizontal overflow at 390px.
- Baseline test reality: repository currently fails when app host/keycloak stack is unavailable or unhealthy; interpret Playwright failures by separating infrastructure readiness errors from genuine UI spec regressions.
- **Cross-agent completion with Yoda & Leia:** Documented design-quality acceptance criteria in decisions.md. Frontend modernization cycle complete: Yoda (architecture) → Leia (implementation) → Wedge (validation gates). All three decision records merged.

### 2025-01-01: Baseline test run — all green
- **Total:** 57 tests | **Passed:** 27 | **Failed:** 0 | **Skipped:** 30
- Build: succeeded (no-build flag works, binaries are up to date)
- **30 skipped tests fall into two categories:**
  1. **Playwright (UI) tests** — require a running app instance. Run with `--filter Category=Playwright`.
  2. **Integration tests (TripApiSpecs, LocationApiSpecs)** — require live Keycloak. Run as part of integration test suite.
- No failures, no build errors. One warning: `NETStandardCompatError_Microsoft_Extensions_Configuration_Binder` — benign compat notice.
- The HTTPS redirect warning (`Failed to determine the https port`) appears during test host startup but does not affect results.
- Pattern: unit/in-process tests (27) run cleanly in CI; Keycloak/Playwright tests are correctly gated behind environment requirements.

### 2025-07-14: Comprehensive Playwright E2E test suite added
**Files created (10 new files):**

#### Page Objects (`PlaywrightSpecs/PageObjects/`)
- `BasePage.cs` — top bar, tab bar, sheet locators; `IsTabActiveAsync`, `IsSheetOpenAsync` helpers
- `LoginPage.cs` — Keycloak login form; `GotoAsync`, `LoginAsync`
- `HomePage.cs` — hero card, ViewTrips link
- `TripListPage.cs` — empty state, FAB, trip items, edit/delete actions; `OpenCreateFormAsync`, `GetTripCountAsync`
- `TripFormPage.cs` — all form fields (Name, Description, StartDate, EndDate, Lat/Long), Submit, Cancel, validation summary

#### Base Setup (`PlaywrightSpecs/PlaywrightSetup.cs`)
- `IAsyncLifetime` base class: creates Chromium with `IgnoreHTTPSErrors=true`, viewport 1280×800
- Credentials from `CAMPLOG_TEST_USER` / `CAMPLOG_TEST_PASSWORD` env vars (default: testuser/testpass)
- `LoginAsync()`, `LoginAndGoToTripsAsync()`, `OpenCreateSheetAsync()`, `CreateTripAsync()` helpers

#### Test Classes
- `AuthTests.cs` — 9 tests covering: unauthenticated redirects, login link visibility, avatar state, Keycloak form presence, post-login app access
- `NavigationTests.cs` — 13 tests covering: tab bar on home/trips, 3-tab count, HomeTab/TripsTab/ProfileTab routing, active-state CSS class, brand link routing
- `TripCrudTests.cs` — 22 tests covering: heading, FAB visibility+position, empty state, card rendering (name/date/chevron), sheet open/dismiss (close btn, cancel, Escape, overlay click), create trip, edit pre-population, edit save, delete
- `MobileViewportTests.cs` — 13 tests at 390×844 (iPhone 14): hero card, tab bar visibility, tab bar fixed+bottom position, bounding box not obscured, all 3 tabs visible, FAB visibility+position+above-tabbar, sheet open, topbar visible

**Total new Playwright tests: 57** (all skipped pending running app + Keycloak)
**Run filter:** `dotnet test --filter "Category=PlaywrightUI"`

**UI features observed:**
- Bottom tab bar (`.app-tabbar`) with 3 tabs: Home `/`, Trips `/Trips`, Profile `/Account/Login`
- Active tab uses `.is-active` CSS class
- FAB `.fab-add-trip` is fixed-position, bottom:80px on mobile
- Slide-up sheet: `#sheet-panel`, `#sheet-overlay`, triggered by `htmx:afterSwap` into `#trip-form-container`
- Trip cards: `.trip-item` > `.trip-main-link` with `h3` (name), `.trip-date-chip` (formatted date), `.trip-snippet` (description), `.trip-chevron`
- Empty state: `.trip-empty-state` with `p` "No trips yet" and `small` instructions
- Create form fields: `Input.Name` (required), `Input.Description`, `Input.StartDate`, `Input.EndDate`, `Input.Latitude`, `Input.Longitude`
- Edit form: same fields plus hidden `Input.Id`; submit button text is "Save" (Create is "Create Trip")
- App URL: `https://localhost:7215`

**Key test structure decisions:**
- Kept existing `AuthSpecs.cs`, `TripSpecs.cs`, `LocationSpecs.cs` unchanged (legacy, wrong URL)
- New tests use Page Object Model and a shared `PlaywrightSetup` base class
- xUnit `IAsyncLifetime` pattern: fresh browser context per test class
- `Guid.NewGuid()` in trip names prevents test cross-contamination
- Tests that need a trip create one on-demand rather than assuming data exists

### 2026-02-26: Login Error Reproduction - OIDC Cookie SameSite Issue

**Tested flow:** Logged out user → clicks protected route (View Trips) → redirected to login → Keycloak auth flow fails with 400 Bad Request

**Error reproduced:**
- **URL chain:** `https://localhost:7215/Account/Login?returnUrl=%2FTrips` → `http://localhost:8080/realms/camplog/protocol/openid-connect/auth?...`
- **Keycloak response:** HTTP 400 Bad Request with "Invalid Request" error message
- **Page ID:** `data-page-id="login-error"`
- **Root cause:** OIDC correlation cookies using `SameSite=None` fail in mixed HTTP/HTTPS dev environment (app on HTTPS, Keycloak on HTTP)

**Current code state (`CampLog.Web\Program.cs` lines 34-35):**
```csharp
options.NonceCookie.SameSite = SameSiteMode.None;
options.CorrelationCookie.SameSite = SameSiteMode.None;
```

**Expected fix (per Chewie's documented solution):**
```csharp
options.NonceCookie.SameSite = SameSiteMode.Lax;
options.CorrelationCookie.SameSite = SameSiteMode.Lax;
```

**CRITICAL FINDING:** Chewie's fix was documented in `.squad/agents/chewie/history.md` but **never actually committed to Program.cs**. The code still has `SameSiteMode.None` on lines 34-35, causing the login error to persist.

**Test environment:**
- Web app: `https://localhost:7215` (running, responding with HTTP 200)
- Keycloak: `http://localhost:8080` (running, realm "camplog" configured)
- AppHost PID: 54048 (CampLog.AppHost running)

**Success criteria validation (pending fix application):**
1. ✅ Reproduced error: 400 Bad Request on Keycloak auth flow with "Invalid Request" message
2. ⏳ Post-fix validation: User should successfully authenticate and return to /Trips
3. ⏳ Log validation: No correlation/nonce cookie errors in application logs

**Next steps:** Chewie needs to apply the documented SameSite=Lax fix to `CampLog.Web\Program.cs` lines 34-35.
