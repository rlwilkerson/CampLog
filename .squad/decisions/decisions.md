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

---

## 7. Ensure Keycloak Logout Includes client_id When id_token_hint Is Cleared

**Date:** 2026-02-27  
**Decider:** Luke (Backend)  
**Status:** Implemented

### Decision

In `CampLog.Web\Program.cs`, explicitly set `context.ProtocolMessage.ClientId = context.Options.ClientId` inside the `OnRedirectToIdentityProviderForSignOut` event handler after clearing `id_token_hint`.

### Rationale

Keycloak requires either `id_token_hint` or `client_id` when `post_logout_redirect_uri` is sent. Clearing `id_token_hint` without adding `client_id` produces invalid logout requests (Invalid IDToken error). The fix ensures sign-out requests include required OIDC protocol parameters.

### Scope

- `CampLog.Web\Program.cs` OpenID Connect configuration
- Event handler: `OnRedirectToIdentityProviderForSignOut`
- No AppHost, Keycloak realm, or API changes

### Consequences

✅ Logout requests include both `client_id` and `post_logout_redirect_uri`  
✅ Resolves ephemeral Keycloak restart logout failures  
✅ Maintains existing id_token_hint clearing for dev stability

---

## 8. Frontend Modernization Architecture — CampLog Web

**Date:** 2026-02-28  
**Decider:** Yoda (Lead Architect)  
**Status:** Implemented

### Decision

Modernize CampLog's frontend from minimal Pico CSS styling to a comprehensive design system maintaining the Dusty Summer color palette while introducing contemporary layout patterns, typography hierarchy, and responsive behavior.

### Architecture Overview

**Design System Components:**
1. **Typography Hierarchy:** System font stack with scaled H1-H3 (1.75→2.25rem), improved body line-height (1.5)
2. **Spacing System:** Extended scale from existing base (0.35–2.6rem) with added `--space-2xl` (3.6rem) for hero sections
3. **Component Conventions:** Standardized cards (border, shadow, padding), button states, form focus, empty states
4. **Layout Discipline:** Preserved 700px breakpoint; expanded desktop to max-width containers; sidebar ≥700px remains 86px vertical
5. **Accessibility:** Maintained 4.5:1 contrast (WCAG AA), 44px touch targets, focus states
6. **Color Preservation:** All Dusty Summer hex values unchanged; gradients/shadows/overlays use same colors at varying opacity

**Pages Inventory:** 19 pages across 4 categories (auth, trips, locations, profile) with priority levels. High-priority modernization: Home, Trips/Locations lists, Profile.

### 5-Phase Implementation Roadmap

1. **Phase 1 (Tokens/Globals):** CSS variables, typography base, global card styles, form improvements
2. **Phase 2 (Navigation):** Topbar refinement, tabbar modernization, mobile-to-desktop transition
3. **Phase 3 (Content Pages):** Home hero polish, trips list refinement, profile page enhancement
4. **Phase 4 (Forms/Modals):** Trip/location forms, bottom sheet animation, confirmation modals
5. **Phase 5 (Edge Cases):** Delete confirmations, privacy page, error page consistency

### Rationale

Delivers cleaner, more modern experience across core pages with minimal risk by preserving existing routes, selectors, HTMX behavior, and Pico CSS foundation.

### Consequences

✅ Enhanced visual hierarchy and responsive behavior  
✅ Improved typography readability and spacing consistency  
✅ Modernized component appearance (cards, buttons, forms)  
✅ Desktop sidebar at 700px breakpoint with improved navigation  
⚠️ CSS-only updates; no breaking changes to Razor Pages or HTMX flows  
⚠️ Requires validation testing across mobile/tablet/desktop viewports

---

## 9. Frontend Modernization Implementation — Leia

**Date:** 2026-02-28  
**Implementer:** Leia (Frontend)  
**Status:** Implemented

### Decision

Apply modernized frontend refresh by enhancing visual hierarchy, spacing, card depth, and responsive polish while preserving Dusty Summer palette and existing HTMX behavior.

### Scope Applied

**CSS Updates:**
- `CampLog.Web\wwwroot\css\camplog.css` with modernized typography scale, layered backgrounds, elevated card styles, refined nav/tab treatment

**Page Refinements (7 pages):**
- `CampLog.Web\Pages\Index.cshtml` (home hero)
- `CampLog.Web\Pages\Trips\Index.cshtml` (trip list)
- `CampLog.Web\Pages\Trips\Locations\Index.cshtml` (location list)
- `CampLog.Web\Pages\Account\Profile.cshtml` (profile)
- `CampLog.Web\Pages\Account\Register.cshtml` (register)
- `CampLog.Web\Pages\Trips\Delete.cshtml` (trip delete)
- `CampLog.Web\Pages\Trips\Locations\Delete.cshtml` (location delete)

### Rationale

Delivers cleaner, more modern experience across core pages with minimal risk by preserving existing routes, selectors, and HTMX interaction patterns. CSS-only improvements maintain backward compatibility.

### Consequences

✅ Modern typography hierarchy applied across all pages  
✅ Improved card elevations and visual depth  
✅ Enhanced spacing consistency and component polish  
✅ Responsive behavior maintained for mobile/tablet/desktop  
✅ HTMX flows and Playwright selectors preserved  
⚠️ No breaking changes; all existing routes and selectors intact

---

## 10. Frontend Redesign Acceptance Test Gates — Wedge

**Date:** 2026-02-28  
**Decider:** Wedge (QA)  
**Status:** Decided

### Decision

Add explicit design-quality acceptance criteria for frontend modernization via new Playwright test suite (`FrontendRedesignAcceptanceTests.cs`).

### Pass/Fail Criteria for Leia

1. **Palette Lock:** CSS custom properties preserve Dusty Summer tokens exactly
   - `--color-salmon #E3AA99`
   - `--color-rose #CD9F8F`
   - `--color-terracotta #DC7147`
   - `--color-amber #D8A748`
   - `--pico-primary-inverse #FFFFFF`

2. **Home Readability:** Hero heading and body meet minimum typographic/readability thresholds (font scale, line-height, card padding)

3. **Mobile Navigation:** Bottom tab bar fixed at bottom with 3 tabs; each tab target ≥44×44

4. **Desktop Navigation:** Tab nav becomes persistent left rail with constrained width and visible divider

5. **Trip Cards:** Cards maintain modern hierarchy (radius, border, internal spacing); date chip preserves amber color (`rgb(216, 167, 72)`)

6. **Responsive Safety:** `/`, `/Trips`, `/Account/Profile` must not introduce horizontal scrolling at 390px width

### Rationale

Codify design-quality gates directly in acceptance test suite to prevent future palette drift, typography regression, or responsive layout breakage. Separates design validation from infrastructure/auth test failures.

### Consequences

✅ Explicit pass/fail criteria prevent future style regressions  
✅ Palette-lock prevents accidental color modifications  
✅ Touch target and responsive requirements enforced automatically  
⚠️ Requires Playwright test environment (Keycloak/AppHost availability)
