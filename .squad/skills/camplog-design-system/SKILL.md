---
name: camplog-design-system
description: CampLog frontend design system for modern, accessible web interfaces preserving Dusty Summer palette. Use this skill when implementing UI pages, components, or styling for CampLog.Web.
license: CampLog project
---

# CampLog Design System Skill

This skill encodes the modern design system for CampLog — an RV trip tracking application with mobile-first Razor Pages + HTMX frontend.

## Design Philosophy

**Aesthetic:** Refined minimalism with warm, organic colors. Clean typography hierarchy, generous spacing, smooth interactions. Think modern travel app — clear, purposeful, inviting.

**Constraint:** Dusty Summer color palette is immutable:
- `#E3AA99` (Salmon)
- `#CD9F8F` (Rose)
- `#DC7147` (Terracotta) — primary action color
- `#D8A748` (Amber) — secondary accent
- `#FFFFFF` (White) — backgrounds
- `#4e3025` (Ink) — text

**Foundation:** Pico CSS v2 as semantic base; extended via `camplog.css` with custom typography, spacing, components, and responsive layouts.

---

## Principles

1. **Mobile-first:** Design for 375px viewport first; enhance at 700px (tablet) and 1200px (desktop) breakpoints.
2. **Spacing discipline:** Use CSS custom properties (`--space-xs` through `--space-2xl`); never hardcode px values.
3. **Typography hierarchy:** System font stack; clear H1-H3 scaling; consistent line-height (1.5 for body, 1.3 for headings).
4. **Accessibility:** 4.5:1 contrast (WCAG AA), 44px touch targets, visible focus states, semantic HTML.
5. **No new frameworks:** CSS-only styling; HTMX for interactions (no client-side JS logic outside of `_Layout.cshtml`).
6. **Pico compatibility:** All custom styles layer on top; never reset Pico defaults unless documented reason.

---

## Design Tokens

### Colors
```css
--pico-primary: #DC7147;
--pico-primary-hover: #CD9F8F;
--pico-primary-focus: rgba(220, 113, 71, 0.25);
--pico-primary-inverse: #FFFFFF;
--pico-secondary: #D8A748;
--pico-secondary-hover: #CD9F8F;
--pico-border-color: rgba(220, 113, 71, 0.22);

--color-salmon: #E3AA99;
--color-rose: #CD9F8F;
--color-terracotta: #DC7147;
--color-amber: #D8A748;
--color-ink: #4e3025;
```

### Spacing
```css
--space-2xs: 0.35rem  /* 4px *)
--space-xs: 0.6rem    /* 8px *)
--space-sm: 0.9rem    /* 12px *)
--space-md: 1.2rem    /* 16px *)
--space-lg: 1.8rem    /* 24px *)
--space-xl: 2.6rem    /* 32px *)
--space-2xl: 3.6rem   /* 48px *)
```

### Typography
```css
/* Font stack (system fonts for performance) */
font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;

/* Heading: 700 weight, 1.3 line-height */
h1: 1.75rem (mobile) → 2.25rem (desktop), 700 weight
h2: 1.5rem (mobile) → 1.75rem (desktop), 700 weight
h3: 1.25rem (mobile) → 1.4rem (desktop), 700 weight

/* Body: 400 weight, 1.5 line-height */
body: 0.95rem (mobile) → 1rem (desktop), 400 weight

/* Small/caption: 500 weight, 0.02em tracking */
small: 0.8rem, 500 weight, 0.02em letter-spacing
```

---

## Component Patterns

### Cards
**Use:** Lists (trips, locations), panels, hero sections

**Structure:**
```html
<article class="card">
  <!-- content -->
</article>
```

**Styling:**
```css
.card {
  border: 1px solid rgba(205, 159, 143, 0.35);
  border-radius: 1rem;
  background: rgba(255, 255, 255, 0.96);
  box-shadow: 0 4px 12px rgba(78, 48, 37, 0.08);
  padding: var(--space-md);
  transition: box-shadow 160ms ease, transform 160ms ease;
}

.card:hover {
  box-shadow: 0 8px 20px rgba(78, 48, 37, 0.15);
  transform: translateY(-2px);
}

.card--interactive {
  cursor: pointer;
}
```

**When:** Trip items, location items, panels, info boxes.

---

### Buttons
**States:**
- **Primary:** Solid terracotta (`#DC7147`), white text
- **Secondary:** Transparent, terracotta border, terracotta text
- **Tertiary/Ghost:** Transparent, inherit text color

**Sizing:**
- Default: `0.6rem 1rem` (padding)
- Compact: `0.5rem 0.8rem`

**Focus state:** `outline: 2px solid #DC7147; outline-offset: 2px;`

**Hover:** Background lightens or shadow increases (context-dependent).

**When:** Form submissions, actions, CTAs.

---

### Forms & Inputs
**Structure:**
```html
<form class="form-grid">
  <fieldset>
    <label>Field Label</label>
    <input type="text" placeholder="..." />
  </fieldset>
</form>
```

**Styling:**
- Input border: `1px solid rgba(205, 159, 143, 0.5)`
- Input background: `#FFFFFF`
- Input padding: `0.6rem 0.8rem`
- Input radius: `0.5rem`
- Focus: `outline: 2px solid #DC7147; outline-offset: 2px;`

**Mobile:** Full-width fields, stacked vertically.
**Desktop (≥700px):** 2-column grid if appropriate (use `.form-grid` and adjust).

**When:** Create/edit trips, create/edit locations, profile fields.

---

### Navigation
**Topbar (56px fixed, sticky):**
- Background: `rgba(255, 255, 255, 0.96)` with `backdrop-filter: blur(6px)`
- Border-bottom: `1px solid rgba(220, 113, 71, 0.15)`
- Brand link: Terracotta, 1.15rem, left-aligned
- User avatar/login: Right-aligned, 2rem circle with light background

**Tabbar (mobile):**
- 68px fixed bottom, 3 tabs equally spaced
- Icon (emoji) + label below
- Active: Background highlight + terracotta text
- Inactive: Muted text (`rgba(78, 48, 37, 0.55)`)
- Transition: `color 150ms ease, background-color 150ms ease`

**Sidebar (desktop, ≥700px):**
- 86px vertical left, starts below topbar
- Same 3 tabs, icon + label stacked vertically
- Active state same as mobile tabbar

**When:** All pages use shared layout via `_Layout.cshtml`.

---

### Empty States
**Structure:**
```html
<section class="empty-state">
  <p>Icon or emoji</p>
  <h3>Empty State Heading</h3>
  <p>Description text</p>
  <a role="button">Add Item</a>
</section>
```

**Styling:**
- Centered container, padding `calc(var(--space-xl) * 1.5) var(--space-md)`
- Border: `1px dashed rgba(205, 159, 143, 0.7)`
- Background: `rgba(227, 170, 153, 0.16)`
- Text color: Muted ink (`rgba(78, 48, 37, 0.6)`)
- Heading: H3, terracotta
- CTA button: Primary style

**When:** Trip list with no trips, location list with no locations.

---

### Modals/Sheets (Bottom Sheet)
**Trigger:** HTMX `hx-get` on form endpoints populates `#trip-form-container`.

**Structure:**
```html
<div id="sheet-overlay" class="sheet-overlay"></div>
<aside id="sheet-panel" class="sheet-panel" role="dialog">
  <span class="sheet-handle"></span>
  <header class="sheet-header">
    <h2>Modal Title</h2>
    <button class="sheet-close">✕</button>
  </header>
  <div id="trip-form-container" class="sheet-content">
    <!-- Form loads here -->
  </div>
</aside>
```

**Styling:**
- Overlay: `rgba(20, 18, 17, 0.45)`, fade in 220ms
- Sheet: Rounded top (`1.5rem`), slide up from bottom, shadow `0 -16px 30px rgba(0, 0, 0, 0.18)`
- Handle: Small gray bar at top center (visual affordance for drag)
- Close button: `2rem × 2rem` circle, light background, centered top-right

**When:** Trip/Location create/edit forms, confirmations.

---

### Badges & Chips
**Trip date chip:**
- Background: `#D8A748` (Amber)
- Text: White, `0.72rem`, 600 weight
- Padding: `0.15rem 0.55rem`
- Border-radius: `999px`

**When:** Trip date display on trip cards.

---

### Gradients
**Body background:**
```css
background: linear-gradient(180deg, rgba(227, 170, 153, 0.3) 0%, #ffffff 32%);
```

**Hero/accent gradient:**
```css
background:
  radial-gradient(circle at top right, rgba(216, 167, 72, 0.18), transparent 55%),
  #ffffff;
```

**Avatar gradient:**
```css
background: linear-gradient(145deg, var(--color-salmon), var(--color-terracotta));
```

---

## Responsive Strategy

### Mobile-First (< 700px)
- Single-column layout
- Full-width cards, no max-width constraint
- Bottom tabbar (68px)
- Sticky topbar (56px)
- Padding: `--space-md` (1.2rem)

### Tablet/Desktop (≥ 700px)
- Left sidebar (86px vertical tabbar)
- Main content area with optional max-width (900px recommended)
- Card grids: 2-column for lists
- Padding: `--space-lg` to `--space-xl`
- Form grids: 2-column when appropriate

### Media Query
```css
@media (min-width: 700px) {
  /* Desktop adjustments */
}

@media (min-width: 1200px) {
  /* Wide desktop (optional) */
}
```

---

## Implementation Workflow

1. **Start with tokens:** Extend or verify CSS variables in `camplog.css`.
2. **Global styles:** Typography base, body background, foundational spacing.
3. **Layout:** Topbar, tabbar/sidebar, main container, safe areas.
4. **Components:** Cards, buttons, forms, empty states, modals.
5. **Pages:** Apply component patterns to specific pages (Home, Trips, Locations, Profile).
6. **Polish:** Hover states, focus states, transitions, shadows.
7. **Validate:** Mobile (375px), tablet (700px), desktop (1200px); contrast; touch targets; a11y.

---

## Common Patterns

### Trip Card (reusable pattern)
```html
<article class="trip-item card">
  <a class="trip-main-link" href="...">
    <span class="trip-avatar">T</span>
    <div class="trip-card-copy">
      <h3>Trip Name</h3>
      <span class="trip-date-chip">MMM d</span>
      <p class="trip-snippet">Description...</p>
    </div>
    <span class="trip-chevron">›</span>
  </a>
  <footer class="trip-card-actions">
    <a class="muted-action">✏️ Edit</a>
    <a class="muted-action">🗑️ Delete</a>
  </footer>
</article>
```

**CSS:**
- `.trip-avatar`: 64px circle gradient
- `.trip-card-copy`: Flex column, min-width 0
- `.trip-date-chip`: Amber badge
- `.trip-snippet`: Single-line ellipsis
- `.trip-chevron`: Right chevron, muted

---

### Page Header (reusable pattern)
```html
<section class="page-header">
  <h1>Page Title</h1>
  <p>Subtitle or description</p>
</section>
```

**CSS:**
- Flex column, gap `--space-sm`
- Margin-bottom `--space-md`

---

## Accessibility Checklist

- [ ] Color contrast ≥ 4.5:1 (WCAG AA) for all text
- [ ] Focus states visible and distinguishable
- [ ] Touch targets ≥ 44px × 44px
- [ ] Semantic HTML (nav, main, article, section, etc.)
- [ ] ARIA labels where needed (buttons, icons, modals)
- [ ] Form labels associated with inputs (`<label for="...">`)
- [ ] Alt text for images (if any)
- [ ] No keyboard traps
- [ ] Screen reader tested (if possible)

---

## Testing Checklist

- [ ] Mobile (375px): Single column, bottom tabbar, readable text
- [ ] Tablet (700px): Sidebar appears, 2-column grid active
- [ ] Desktop (1200px): Max-width containers active if used
- [ ] Hover states work (card lift, button highlight)
- [ ] Focus states visible (outline clear)
- [ ] Form validation feedback clear
- [ ] Empty states display when no data
- [ ] Modals open/close smoothly
- [ ] No horizontal scroll on any viewport
- [ ] Safe areas respected (notch devices)

---

## Resources

- **Pico CSS:** https://picocss.com/
- **WCAG A11y:** https://www.w3.org/WAI/WCAG21/quickref/
- **System fonts:** https://systemfontstack.com/
- **CampLog decision:** `.squad/decisions/inbox/yoda-frontend-modernization.md`

---

**Owner:** Yoda (Lead Architect)  
**Last Updated:** 2025-02-28  
**Status:** Active
