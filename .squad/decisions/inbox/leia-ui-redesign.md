# Leia UI Redesign Direction

## Summary
Adopt a mobile-first, card-based Pico CSS layout across CampLog while preserving the Dusty Summer palette.

## Direction Chosen
- Replace dense table/list presentation with stacked cards on mobile that scale to two columns on larger viewports.
- Use a clean shell with sticky header, restrained gradients, and consistent spacing tokens for better visual rhythm.
- Standardize form and action patterns (`form-grid`, `form-actions`, `page-header`, `panel-card`) so all Razor Pages feel cohesive.
- Keep HTMX behavior unchanged (trip create still renders into `#trip-form-container`) while improving visual integration of partial content.

## Why
The previous UI was functional but visually flat and cramped on small screens due to table-heavy structure and inconsistent spacing.
This direction improves readability, touch ergonomics, and polish without introducing new frameworks or changing brand colors.
