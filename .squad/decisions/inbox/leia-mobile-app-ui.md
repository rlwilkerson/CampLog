# Leia Mobile App UI Redesign

**By:** Leia  
**Requested by:** Rick Wilkerson

## Decision
Implement the CampLog web UI with a native-mobile app shell pattern on Trips-focused flows:

1. Replace top navigation links with a minimal sticky top app bar (brand + auth/profile affordance) and move primary navigation to a fixed bottom tab bar (Home, Trips, Profile) with terracotta active state.
2. Add a global slide-up sheet system in `_Layout.cshtml` (`#sheet-overlay`, `#sheet-panel`, `#trip-form-container`) that opens when HTMX swaps trip form partials and closes via backdrop, close button, cancel action, or Escape.
3. Replace the Trips page inline "Add Trip" button with a floating action button (56x56, terracotta, fixed above tab bar) that launches the sheet via HTMX.
4. Redesign trip list cards to app-style rows with a 64px avatar tile, bold title, amber date chip (`MMM d`), one-line snippet, chevron affordance, and muted inline edit/delete actions.
5. Use a minimal, centered text empty state for zero trips and remove dashed empty-state chrome from the Trips page only.
6. Keep all work within Razor + HTMX + Pico/custom CSS and preserve Dusty Summer color tokens exactly as defined.

## Why
This structure better matches modern iOS/Android interaction patterns, improves one-hand mobile usability, and preserves existing Razor Pages/HTMX architecture without backend changes.
