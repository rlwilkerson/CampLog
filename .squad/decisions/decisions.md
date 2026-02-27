# Decision Log

## 1. Keycloak Realm Persistence & Audience Validation Fix

**Date:** 2025-01-23  
**Decider:** Chewie (DevOps/Infra)  
**Status:** Implemented  

### Decision

Remove `.WithDataVolume()` from Keycloak resource in AppHost.cs to ensure realm JSON is always freshly imported. Add `oidc-audience-mapper` to `camplog-web` client in camplog-realm.json to inject required `aud` claim.

### Rationale

1. **WithDataVolume() removal**: Ephemeral storage ensures configuration changes are reproducible; eliminates "realm already exists" skips
2. **Audience mapper**: API validates `Audience = "camplog-web"` on JWT tokens; Keycloak 26.x requires explicit protocol mapper to include client ID in `aud` claim

### Consequences

✅ Realm configuration changes apply on every Aspire restart  
✅ JWT tokens include required `"aud": ["camplog-web"]`  
⚠️ Keycloak state (users, sessions) not persisted across restarts in local dev (acceptable; test users are in realm JSON)

---

## 2. Keycloak Test User for Playwright E2E Tests

**Date:** 2025  
**Author:** Chewie (DevOps/Infra)  
**Status:** Decided

### Decision

Create permanent test user in `camplog` Keycloak realm:
- **Username:** testuser
- **Email:** testuser@camplog.test (preferred for login; realm uses `registrationEmailAsUsername: true`)
- **Password:** testpass
- **Enabled:** true
- **Credentials:** permanent

### Rationale

E2E test suite needs stable credentials to authenticate as real user for authenticated flows.

### Consequences

- User recreated on each Keycloak container restart
- Tests should create/verify user idempotently
- Admin password changes on restart; scripts must read from container env

---

## 3. Remove API HTTPS Redirection for Internal Aspire Traffic

**Date:** 2026-02-27  
**Decider:** Luke (Backend)  
**Status:** Implemented

### Decision

Remove `app.UseHttpsRedirection();` from `CampLog.Api/Program.cs`.

### Rationale

Web calls API over `http://api`. HTTP→HTTPS redirect (307) triggers .NET HttpClient to follow cross-scheme redirect, which strips Authorization header per security policy. Removing redirect preserves token on direct HTTP service-to-service calls and resolves 401 errors.

### Scope

- `CampLog.Api/Program.cs` only
- No AppHost changes
- No frontend contract changes

---

## 4. Mobile App UI Pattern with Bottom Tab Navigation

**Date:** 2025  
**Author:** Leia (Frontend)  
**Status:** Decided

### Decision

Implement trips-focused flows with native mobile app shell pattern:

1. Sticky top app bar (brand + auth) + fixed bottom tab bar (Home, Trips, Profile)
2. Global slide-up sheet system for trip form dialogs (HTMX swaps into `#trip-form-container`)
3. Floating action button (56×56, terracotta) for "Add Trip" above tab bar
4. Trip list cards: 64px avatar, bold title, amber date chip (MMM d), one-line snippet, chevron
5. Minimal text empty state; remove dashed chrome
6. Stay within Razor + HTMX + Pico CSS; preserve Dusty Summer palette

### Rationale

Better matches modern iOS/Android patterns; improves one-hand mobile usability; preserves Razor Pages/HTMX architecture.

---

## 5. UI Design Direction: Mobile-First Cards with Pico CSS

**Date:** 2025  
**Author:** Leia (Frontend)  
**Status:** Decided

### Decision

Adopt mobile-first card-based Pico CSS layout across CampLog:
- Replace dense tables/lists with stacked cards (mobile) → two columns (large viewports)
- Sticky header + consistent spacing tokens
- Standardize form patterns: `form-grid`, `form-actions`, `page-header`, `panel-card`
- Keep HTMX behavior unchanged; improve visual integration

### Rationale

Previous UI was functionally correct but visually flat and cramped on small screens.

---

## 6. Playwright E2E Test Suite Setup

**Date:** 2025-07-14  
**Author:** Wedge (QA)  
**Status:** Accepted

### Decision

Add comprehensive Playwright E2E tests to existing `CampLog.Tests` project:
- Base URL: `https://localhost:7215` (Aspire HTTPS port)
- SSL: `IgnoreHTTPSErrors = true`
- Framework: xUnit with `IAsyncLifetime`
- Structure: Page Object Model in `PlaywrightSpecs/PageObjects/`
- Test credentials: env vars `CAMPLOG_TEST_USER` / `CAMPLOG_TEST_PASSWORD` (default: testuser/testpass)
- Skip policy: All 57 tests marked `[Fact(Skip = "...")]`; run with `--filter "Category=PlaywrightUI"`
- Data isolation: Each test creates unique test data; no assumptions about pre-existing state

### Rationale

Establish consistent E2E testing pattern supporting mobile-first UI redesign with proper POM structure.

### Consequences

- 57 new E2E test cases across 4 classes
- All tests skipped in normal CI; enabled via filter when app+Keycloak available
- Legacy stubs (AuthSpecs, TripSpecs, LocationSpecs) left unchanged
