# Decision: Playwright E2E Test Suite Setup

**By:** Wedge (QA)
**Date:** 2025-07-14
**Status:** Accepted

## Context
CampLog.Tests had 3 placeholder Playwright spec files (AuthSpecs, TripSpecs, LocationSpecs) that were basic, used the wrong base URL (`http://localhost:5000`), had no Page Object Model structure, and didn't cover all UI features of the redesigned mobile-first UI.

## Decision

### Test project
Added Playwright tests to the **existing** `CampLog.Tests` project — no new project needed. `Microsoft.Playwright` 1.58.0 was already a dependency.

### Base URL
`https://localhost:7215` — the correct Aspire-assigned HTTPS port for the web app.

### SSL
`IgnoreHTTPSErrors = true` on `BrowserNewContextOptions` — required for localhost dev certs.

### Test framework
**xUnit** — already in the project. Used `IAsyncLifetime` for browser setup/teardown per test class. No NUnit/MSTest migration needed.

### Page Object Model
Implemented in `PlaywrightSpecs/PageObjects/`:
- `BasePage` (layout: topbar, tabbar, sheet)
- `LoginPage` (Keycloak fields)
- `HomePage`
- `TripListPage`
- `TripFormPage`

### Test class location
`CampLog.Tests/PlaywrightSpecs/` — new files alongside the legacy stubs (stubs left untouched).

### Test credentials
Read from environment variables `CAMPLOG_TEST_USER` / `CAMPLOG_TEST_PASSWORD`, defaulting to `testuser` / `testpass` for local dev.

### Test data isolation
Each test that needs a trip creates one with a `Guid`-based name. Tests never assume pre-existing data state.

### Skip policy
All 57 new tests are marked `[Fact(Skip = "...")]` — they require a running app with Keycloak. Run them with:
```
dotnet test --filter "Category=PlaywrightUI"
```

### Legacy stubs
`AuthSpecs.cs`, `TripSpecs.cs`, `LocationSpecs.cs` left unchanged. They use `http://localhost:5000` and stale selectors. They should be replaced with the new comprehensive suite when the team is ready.

## Consequences
- 57 new E2E test cases organized across 4 test classes
- Full coverage: auth, navigation, trip CRUD, mobile viewport (390×844)
- All tests skipped in normal CI; enabled via filter when app+Keycloak are available
- Page Object Model enables easy selector updates if UI changes
