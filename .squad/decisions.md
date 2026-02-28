# CampLog — Team Decisions

## Active Decisions

### 2025-01-01: Team roster established
**By:** Rick Wilkerson
**What:** Team cast from Star Wars OT universe. Han and Obi-Wan retired. Active team: Yoda (Lead), Luke (Backend), Leia (Frontend), Chewie (DevOps), Wedge (QA).
**Why:** Owner-specified roster.

### 2025-01-01: Tech stack locked
**By:** Rick Wilkerson (via Prompt.md)
**What:** .NET 10, .NET Aspire (CLI), Razor Pages + HTMX + Pico CSS, Keycloak, PostgreSQL
**Why:** Owner-specified stack.

### 2025-01-01: Color theme established
**By:** Rick Wilkerson
**What:** Dusty Summer palette — #E3AA99, #CD9F8F, #DC7147, #D8A748, #FFFFFF
**Why:** Owner-specified design tokens.

### 2025-01-01: Mobile-first requirement
**By:** Rick Wilkerson
**What:** All UI must be mobile-friendly and responsive.
**Why:** Owner-specified requirement.

### 2025-01-27: OIDC Login Redirect Fix
**By:** Chewie (DevOps/Infra)
**What:** Configure OIDC and authentication cookies to use `SameSite=Lax` in development to handle mixed HTTP/HTTPS authentication flows.
**Why:** ASP.NET Core OIDC middleware's default `SameSite=None; Secure=true` breaks when authentication involves HTTP (Keycloak dev) redirecting through HTTPS (app). `SameSite=Lax` allows cookies on top-level navigation across protocol boundaries while keeping CSRF protection.
**Impact:** Fixes authentication redirect chain in dev environment. Production should enforce stricter `SameSite=None; Secure=true` when both app and Keycloak use HTTPS.

### 2026-02-27: Preserve local login return URLs in frontend navigation
**By:** Leia (Frontend)
**What:** Unauthenticated UI login entry points now include `returnUrl` query parameter. Only local app routes accepted; falls back to `/`.
**Why:** Post-logout trip navigation could produce unsafe redirect targets. Keeping return targets local and explicit prevents broken post-login routing and avoids unsafe redirects.

### 2026-02-27: Sub claim resolution in API
**By:** Luke (Backend)
**What:** Update `CampLog.Api\Extensions\ClaimsPrincipalExtensions.cs` to resolve user ID from `sub` (raw JWT claim) or `ClaimTypes.NameIdentifier` (mapped subject claim).
**Why:** Aspire API logs show 500 errors on `/trips` when authenticated requests contain mapped claims instead of raw `sub`. Accepting the mapped subject claim preserves strict auth expectations while supporting Keycloak token handling in ASP.NET claim mapping.
**Impact:** Prevents erroneous 500s for valid Keycloak-authenticated users without introducing broad catches or silent fallbacks.

### 2026-02-28T04:40:53.300Z: User directive
**By:** Rick Wilkerson (via Copilot)
**What:** Create a new web UI project named CampLog.Web2 using Razor Pages, Pico CSS, and HTMX. Include modern layout plus Login/Logout/Register and Trips CRUD pages.
**Why:** User request — captured for team memory

### 2026-02-28T04:56:08.545Z: User directive
**By:** Rick Wilkerson (via Copilot)
**What:** Use CampLog.slnx as the solution file name.
**Why:** User request — captured for team memory

### 2026-02-28: Restart web project creation with safe parallel clone
**By:** Leia (Frontend)
**What:** Created `CampLog.Web2` by cloning the current modernized `CampLog.Web` UI stack and added `CampLog.Web2/CampLog.Web2.csproj` to `CampLog.slnx`.
**Why:** This restarts the "new web project" effort with minimal risk, preserves current production-facing behavior, and provides a dedicated frontend workspace for future Wedge-spec-driven changes.
**Blockers:** No Web2-specific Playwright specs from Wedge yet, so net-new UI divergence in `CampLog.Web2` is paused until those specs are provided.
