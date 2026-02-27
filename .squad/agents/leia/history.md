# Leia — Project History

## Core Context
**Project:** CampLog — RV trip tracking application
**Owner:** Rick Wilkerson
**Stack:** Razor Pages (.cshtml), HTMX, Pico CSS, .NET 10

**Color palette (Dusty Summer):**
- #E3AA99 (warm blush)
- #CD9F8F (muted rose)
- #DC7147 (terracotta/orange)
- #D8A748 (golden amber)
- #FFFFFF (white)

**UI responsibilities:** All pages (register, login, trip list, trip detail, location forms), mobile-first layout

**HTMX patterns:** Partial updates for trip/location lists, inline form editing, no full page reloads

## Learnings
<!-- Append Razor Page patterns, HTMX snippets, layout decisions below -->
- 2026-02-25: Shifted CampLog to a mobile-first card layout for trips and locations, replacing wide tables with stacked article cards and grouped metadata so actions stay readable on phone screens.
- 2026-02-25: Established shared UI utility patterns (`page-header`, `panel-card`, `trip-list`, `location-list`, `form-grid`, `form-actions`, `empty-state`) to keep Razor Pages visually consistent while preserving HTMX partial rendering.
- 2026-02-25: Refined the Dusty Summer theme by tuning Pico variables and shell structure (sticky header, improved spacing hierarchy, softer surfaces, and stronger typography contrast) without changing the approved palette.
- 2026-02-25: Reframed the shell into a native-app pattern with a 56px top app bar, fixed bottom tab bar, and a global HTMX-driven slide-up sheet (`#sheet-overlay`, `#sheet-panel`, `#trip-form-container`) controlled by lightweight vanilla JS class toggles.
- 2026-02-25: Redesigned the Trips index for mobile app ergonomics using a terracotta FAB, 80px+ tappable trip cards (avatar block, date chip, one-line snippet, chevron, muted quick actions), and a minimal text-only empty state.
- 2026-02-25: Converted Trips create/edit Razor views into sheet-friendly form partials (no outer chrome, keep hx-* flows) with in-sheet cancel controls via `data-close-sheet` so form interactions stay inside the bottom sheet UX.
- 2026-02-27: Added an authenticated profile destination in the shell (avatar + Profile tab route to `/Account/Profile`) and introduced a mobile-first profile page that surfaces display name, email, and a Dusty Summer-styled logout action.
- 2026-02-27: Standardized login links to carry a local `returnUrl` (including current path/query) and hardened `/Account/Login` to only honor local redirects, fixing post-logout trip navigation login returns.
- 2026-02-26: **Cross-agent fix with Chewie:** Frontend login returnUrl propagation now works with Chewie's OIDC cookie fix (`SameSite=Lax`). Together these solve the complete redirect chain: auth succeeds + user returns to intended destination. Session log: `.squad\log\20260226-220105-login-redirect-fix.md`
