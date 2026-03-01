# CampLog.Web — Application Specification

This document describes the CampLog.Web frontend in enough detail to recreate it with a different visual design, layout framework, or technology stack while preserving all functional behavior.

---

## 1. Project Overview

**CampLog** is an RV trip tracking web application. Users log in, create trips, and add campsite locations to each trip. The app is designed as a **mobile-first progressive web app** with a native app feel (sticky header, bottom tab bar on mobile, sheet panels for forms).

**Core user flows:**
1. Land on the home page → see the pitch → click "View Trips"
2. If not authenticated → redirected to login → Keycloak OIDC → land on Trips
3. Browse trips list → tap a trip → see its campsite locations
4. Add / edit / delete trips (sheet panel overlay on mobile)
5. Add / edit / delete locations (full page form)
6. View profile → log out

---

## 2. Technology Stack

| Layer | Technology |
|-------|------------|
| Framework | ASP.NET 10 Razor Pages |
| CSS | Pico CSS v2 (CDN) + custom `camplog.css` |
| Partial updates | HTMX 2.0.4 (CDN) |
| Authentication | Keycloak via OIDC (OpenID Connect) + cookie session |
| API communication | `IHttpClientFactory` named client `"api"`, Bearer token forwarding |
| Service discovery | .NET Aspire (`AddServiceDiscovery`) — base URL `https://api` resolves at runtime |
| Orchestration | .NET Aspire AppHost |

---

## 3. Application Shell

### 3.1 HTML Structure

Every page renders inside the shared layout (`_Layout.cshtml`) with this structure:

```
<html>
  <head>
    Pico CSS (CDN)
    camplog.css (custom theme + component styles)
    CampLog.Web.styles.css (scoped CSS)
  </head>
  <body>
    <header class="app-topbar">          ← sticky top bar
    <main class="container site-main">  ← page content
    <nav class="app-tabbar">            ← bottom tab bar (mobile) / left sidebar (≥700px)
    <div id="sheet-overlay">            ← dim overlay for sheet panel
    <aside id="sheet-panel">            ← bottom sheet panel (trip create/edit forms)
      <div id="trip-form-container">    ← HTMX swap target
    </aside>
    HTMX script (CDN)
    inline JS (sheet panel controller)
  </body>
</html>
```

### 3.2 Top Bar (`app-topbar`)

- Sticky, `top: 0`, `z-index: 40`
- Height: `56px`
- Background: `rgba(255,255,255,0.9)` with `backdrop-filter: blur(12px)`
- Bottom border: `1px solid rgba(220,113,71,0.15)`
- Box shadow: `0 8px 24px rgba(78,48,37,0.06)`

**Contents:**
- Left: Brand link "**CampLog**" → `/` — color `#DC7147`, font-size `1.2rem`, no underline
- Right (authenticated): Avatar circle showing user's first initial → `/Account/Profile`
- Right (unauthenticated): `↪` icon link → `/Account/Login?returnUrl={currentPath}`

**Avatar/login icon specs:**
- `2rem × 2rem` circle, `border-radius: 999px`
- Background: `rgba(220,113,71,0.08)`, border: `1px solid rgba(220,113,71,0.3)`
- Color: `#DC7147`, font-weight: `700`, font-size: `0.9rem`

### 3.3 Tab Bar (`app-tabbar`)

Three tabs: **Home** 🏕️, **Trips** 🗺️, **Profile** 👤

**Mobile (< 700px):**
- Fixed to bottom, full width, height `68px`
- Background: `rgba(255,255,255,0.94)` with `backdrop-filter: blur(10px)`
- Top border: `1px solid rgba(205,159,143,0.32)`
- 3-column grid
- Safe area padding for notched devices: `padding-bottom: calc(0.45rem + env(safe-area-inset-bottom))`

**Desktop (≥ 700px):**
- Becomes a left-side vertical sidebar
- Position: `top: 56px`, fixed, width `86px`, full viewport height minus topbar
- Right border instead of top border
- Single-column grid, `align-content: start`

**Active tab:** background `rgba(220,113,71,0.14)`, color `#DC7147`, border-radius `0.8rem`

**Tab routing:**
- Home → active when path is exactly `/`
- Trips → active when path starts with `/Trips`
- Profile → active when path starts with `/Account`

**Profile tab href logic:**
- Authenticated → `/Account/Profile`
- Unauthenticated → `/Account/Login?returnUrl={currentPath}`

### 3.4 Sheet Panel (Bottom Sheet)

Used for **Create Trip** and **Edit Trip** forms on the trips list page. The form is loaded via HTMX into `#trip-form-container`, and the sheet panel opens automatically.

**Panel structure:**
```
<aside id="sheet-panel" class="sheet-panel" role="dialog" aria-modal="true">
  <span class="sheet-handle">          ← drag handle visual affordance
  <header class="sheet-header">
    <h2 id="sheet-title">Trip</h2>
    <button data-close-sheet>✕</button>
  </header>
  <div id="trip-form-container" class="sheet-content">
    ← HTMX loads form partial here
  </div>
</aside>
<div id="sheet-overlay" class="sheet-overlay"></div>
```

**Panel specs:**
- Fixed, `left: 0 right: 0 bottom: 0`
- `z-index: 71`
- Height: `min(90vh, 860px)`
- Background: `#fff`, border-radius `1.5rem 1.5rem 0 0`
- Enters via `translateY(102%)` → `translateY(0)` on `transition: 220ms ease`
- Box shadow: `0 -16px 30px rgba(0,0,0,0.18)`
- Overflow: `auto`

**Overlay:** `z-index: 70`, `rgba(20,18,17,0.45)`, fades opacity `0 → 1` on 220ms ease

**Sheet open trigger:** HTMX `htmx:afterSwap` event on `#trip-form-container` (non-empty content)

**Dismiss mechanisms:**
1. Click overlay
2. Click any element with `data-close-sheet` attribute
3. Press `Escape` key

**Sheet handle:** `3rem × 0.35rem` pill, centered, `background: rgba(78,48,37,0.25)`

### 3.5 Main Content Area

- `class="container site-main"`
- Width: `min(100%, 70rem)`
- Padding top: `var(--space-lg)` (1.8rem)
- Padding bottom: `calc(74px + var(--space-lg))` on mobile (clears tab bar)
- Padding bottom: `var(--space-lg)` on desktop
- Padding left: `calc(86px + var(--space-md))` on desktop (clears sidebar)

---

## 4. Design Tokens

### 4.1 Color Palette — "Dusty Summer"

| Token | Value | Usage |
|-------|-------|-------|
| `--color-salmon` / `--pico-primary-hover` | `#E3AA99` | Hover states, gradients |
| `--color-rose` | `#CD9F8F` | Secondary hover, form borders |
| `--color-terracotta` / `--pico-primary` | `#DC7147` | Primary CTA, active tab, FAB, brand |
| `--color-amber` / `--pico-secondary` | `#D8A748` | Date chips, badges, accents |
| `--color-ink` | `#4e3025` | Body text (dark warm brown) |
| `--color-paper` | `#fffdfb` | Card backgrounds |
| `--pico-primary-inverse` | `#FFFFFF` | Text on primary buttons |
| `--pico-border-color` | `rgba(220,113,71,0.22)` | General borders |

### 4.2 Spacing Scale

| Token | Value |
|-------|-------|
| `--space-2xs` | `0.35rem` |
| `--space-xs` | `0.6rem` |
| `--space-sm` | `0.9rem` |
| `--space-md` | `1.2rem` |
| `--space-lg` | `1.8rem` |
| `--space-xl` | `2.6rem` |

### 4.3 Typography

- Base font size: `0.98rem`
- Line height: `1.55`
- Heading letter-spacing: `-0.01em`
- `h1` font-weight: `750`
- `h2`, `h3` font-weight: `700`
- Heading line-height: `1.2`

### 4.4 Page Background

```css
body {
  background:
    radial-gradient(circle at 15% -10%, rgba(227,170,153,0.55), transparent 32%),
    radial-gradient(circle at 85% 0%,  rgba(216,167,72,0.35),  transparent 28%),
    linear-gradient(180deg, #fff9f5 0%, #ffffff 40%);
}
```

---

## 5. Pages

### 5.1 Home Page (`/`)

**File:** `Pages/Index.cshtml`  
**Auth:** Anonymous allowed  
**Route:** `GET /`

**Purpose:** Marketing/landing page. Directs users to the Trips section.

**Layout:**
```
<article class="hero-card">
  <header>
    <p class="hero-kicker">CampLog</p>
    <h1>Track every stop of your next adventure</h1>
    <p>CampLog keeps trips and campsite locations organized...</p>
    <ul class="hero-metrics">
      <li>Plan faster</li>
      <li>Capture memories</li>
      <li>Stay organized</li>
    </ul>
  </header>
  <footer class="inline-actions">
    <a role="button" href="{tripsHref}">View Trips</a>
    <a role="button" class="secondary" href="/Privacy">Privacy</a>
  </footer>
</article>
```

**"View Trips" href logic:**
- Authenticated → `/Trips`
- Unauthenticated → `/Account/Login?returnUrl=/Trips`

**Hero card specs:**
- Border: `1px solid rgba(220,113,71,0.2)`, border-radius `1.1rem`
- Box shadow: `0 16px 32px rgba(220,113,71,0.1)`
- Background: `radial-gradient(circle at top right, rgba(216,167,72,0.24), transparent 55%), #ffffff`
- Padding: `clamp(1rem, 4vw, 2rem)`

**Kicker (`hero-kicker`):** font-size `0.78rem`, font-weight `700`, `text-transform: uppercase`, letter-spacing `0.08em`, color `rgba(78,48,37,0.68)`

**H1:** `font-size: clamp(1.6rem, 4vw, 2.25rem)`, line-height `1.1`

**Metric pills (`hero-metrics li`):**
- Inline pill: border `1px solid rgba(205,159,143,0.55)`, border-radius `999px`
- Padding `0.2rem 0.65rem`, font-size `0.78rem`, font-weight `600`
- Background `rgba(255,255,255,0.9)`, color `rgba(78,48,37,0.84)`

---

### 5.2 Trips List (`/Trips`)

**File:** `Pages/Trips/Index.cshtml` + `IndexModel`  
**Auth:** `[Authorize]`  
**Route:** `GET /Trips`  
**API call:** `GET /trips` → `List<TripDto>`

**Page header section:**
```
<section class="page-header">
  <h1>My Trips</h1>
  <p>Organize dates, notes, and destinations in one place.</p>
  <small class="page-meta">{N} trip(s)</small>   ← pluralizes correctly
</section>
```

**Empty state (no trips):**
```
<section class="trip-empty-state panel-card">
  <p>No trips yet</p>
  <small>Tap + to add your first trip</small>
</section>
```
- Centered text, color `rgba(78,48,37,0.6)`
- Padding: `calc(var(--space-xl) * 1.2) var(--space-sm)`

**Trip list (has trips):**
```
<div class="trip-list">
  <article class="trip-item"> × N
```

**Desktop:** `trip-list` becomes 2-column grid at ≥ 700px

**Trip card anatomy:**
```
<article class="trip-item">
  <a class="trip-main-link" href="/Trips/Locations/{tripId}">   ← navigates to locations
    <span class="trip-avatar">{FirstLetter}</span>
    <div class="trip-card-copy">
      <h3>{trip.Name}</h3>
      <span class="trip-date-chip">{startDate MMM d | "No date"}</span>
      <p class="trip-snippet">{description truncated to 80 chars or "-"}</p>
    </div>
    <span class="trip-chevron">›</span>
  </a>
  <footer class="trip-card-actions">
    <a class="muted-action" hx-get="/Trips/Edit/{id}" hx-target="#trip-form-container" hx-swap="innerHTML">✏️ Edit</a>
    <a class="muted-action" href="/Trips/Delete/{id}">🗑️ Delete</a>
  </footer>
</article>
```

**Trip avatar:** `64×64px` rounded square (`border-radius: 1rem`), gradient `linear-gradient(145deg, #E3AA99, #DC7147)`, white text, font-size `1.3rem`, font-weight `700`. Fallback emoji `🏕️` if name is empty.

**Date chip:** inline pill, background `#D8A748`, white text, font-size `0.72rem`, border-radius `999px`

**Trip snippet:** single line, `text-overflow: ellipsis`, color `rgba(78,48,37,0.75)`, font-size `0.9rem`

**Chevron:** color `rgba(78,48,37,0.42)`, font-size `1.6rem`

**Trip card hover:** `transform: translateY(-1px)`, enhanced box shadow

**Edit action (HTMX):**
- `hx-get="/Trips/Edit/{id}"` loads form into `#trip-form-container`
- `hx-swap="innerHTML"` replaces container content
- `hx-disabled-elt="this"` disables link during load
- Sheet opens automatically via `htmx:afterSwap` event

**FAB (Floating Action Button):**
```
<button class="fab-add-trip" hx-get="/Trips/Create" hx-target="#trip-form-container" hx-swap="innerHTML">+</button>
```
- Fixed position, `right: 24px`, `bottom: 80px` mobile / `bottom: 24px` desktop
- `56×56px` circle, background `#DC7147`, white text, font-size `2rem`
- `z-index: 44` (below sheet overlay at 70)
- Box shadow: `0 10px 24px rgba(78,48,37,0.3)`
- Hover: `transform: scale(1.05)`

---

### 5.3 Create Trip (Sheet Form)

**File:** `Pages/Trips/Create.cshtml` + `CreateModel`  
**Auth:** `[Authorize]`  
**Route:** `GET /Trips/Create` (HTMX partial) | `POST /Trips/Create`  
**API call on POST:** `POST /trips` with `TripUpsertRequest`

**HTMX partial behavior:** When request has `HX-Request` header, `Layout = null` (renders without shell — just the form fragment).

**On success:** Redirects to `/Trips/Index` (full page redirect after POST).

**Form markup:**
```html
<form method="post" class="sheet-form">
  <div asp-validation-summary="All"></div>
  <label>
    Name
    <input asp-for="Input.Name" required />
  </label>
  <label>
    Description
    <textarea asp-for="Input.Description"></textarea>
  </label>
  <div class="form-grid">
    <label>Latitude <input type="number" step="any" /></label>
    <label>Longitude <input type="number" step="any" /></label>
  </div>
  <div class="form-grid">
    <label>Start Date <input type="date" /></label>
    <label>End Date <input type="date" /></label>
  </div>
  <footer class="form-actions">
    <button type="submit">Create Trip</button>
    <button type="button" class="secondary" data-close-sheet>Cancel</button>
  </footer>
</form>
```

**`form-grid`:** single column, becomes 2-column at ≥ 700px (`grid-template-columns: repeat(2, 1fr)`)

**`sheet-form`:** `display: grid; gap: var(--space-sm)`

**Cancel button** has `data-close-sheet` → closes the sheet panel via JS

---

### 5.4 Edit Trip (Sheet Form)

**File:** `Pages/Trips/Edit.cshtml` + `EditModel`  
**Auth:** `[Authorize]`  
**Route:** `GET /Trips/Edit/{id:guid}` (HTMX partial) | `POST /Trips/Edit/{id:guid}`  
**API calls:** `GET /trips/{id}` on load; `PUT /trips/{id}` on submit

**Identical form layout to Create Trip** with:
- Hidden `<input asp-for="Input.Id" />` (preserves GUID on POST)
- Submit button label: "Save" (not "Create Trip")
- Same HTMX partial / no-layout behavior on `HX-Request` header

---

### 5.5 Delete Trip

**File:** `Pages/Trips/Delete.cshtml` + `DeleteModel`  
**Auth:** `[Authorize]`  
**Route:** `GET /Trips/Delete/{id:guid}` | `POST /Trips/Delete/{id:guid}`  
**API calls:** `GET /trips/{id}` on load; `DELETE /trips/{id}` on confirm

**Full page (not a sheet panel).** No HTMX.

```html
<article class="panel-card confirm-card">
  <h1>Delete Trip</h1>
  <!-- If trip found: -->
  <p>Are you sure you want to delete <strong>{trip.Name}</strong>?</p>
  <form method="post" class="form-actions">
    <button type="submit" class="contrast">Delete</button>
    <a role="button" class="secondary" href="/Trips">Cancel</a>
  </form>
  <!-- If trip not found: -->
  <p>Trip not found.</p>
</article>
```

**`confirm-card`:** `max-width: 34rem`, centered

---

### 5.6 Trip Locations List (`/Trips/Locations/{tripId}`)

**File:** `Pages/Trips/Locations/Index.cshtml` + `IndexModel`  
**Auth:** `[Authorize]`  
**Route:** `GET /Trips/Locations/{tripId:guid}`  
**API call:** `GET /trips/{tripId}/locations` → `List<LocationDto>`

**Page header:**
```
<section class="page-header">
  <h1>Locations</h1>
  <p>Manage each place visited on this trip.</p>
  <small class="page-meta">{N} location(s)</small>
  <a role="button" href="/Trips/Locations/Create/{tripId}">Add Location</a>
</section>
```

**Empty state:**
```html
<article class="empty-state">
  <p>No locations yet.</p>
</article>
```
- Border: `1px dashed rgba(205,159,143,0.7)`, border-radius `0.8rem`
- Background: `rgba(227,170,153,0.16)`

**Location list:** `display: grid; gap: var(--space-md)` — becomes 2-column at ≥ 700px

**Location card:**
```html
<article class="location-item">
  <h3>{location.Name}</h3>
  <p>{location.Description | "No description"}</p>
  <div class="meta-list">
    <small><strong>Dates</strong><span>{startDate yyyy-MM-dd} to {endDate yyyy-MM-dd}</span></small>
    <small><strong>GPS</strong><span>{lat}, {lon}</span></small>
  </div>
  <footer class="inline-actions">
    <a role="button" class="secondary outline" href="/Trips/Locations/Edit/{tripId}/{id}">Edit</a>
    <a role="button" class="contrast outline" href="/Trips/Locations/Delete/{tripId}/{id}">Delete</a>
  </footer>
</article>
```

**Location item styles:**
- Padding: `var(--space-md)` (1.2rem)
- Border: `1px solid rgba(205,159,143,0.35)`, border-radius `0.9rem`
- Background: `rgba(255,255,255,0.98)`
- Box shadow: `0 10px 20px rgba(78,48,37,0.06)`

**`meta-list`:** rows of key/value pairs with `justify-content: space-between`, each row: border `1px solid rgba(205,159,143,0.4)`, border-radius `0.65rem`, padding `0.55rem 0.7rem`, background `rgba(255,255,255,0.72)`

---

### 5.7 Create Location

**File:** `Pages/Trips/Locations/Create.cshtml` + `CreateModel`  
**Auth:** `[Authorize]`  
**Route:** `GET /Trips/Locations/Create/{tripId:guid}` | `POST /Trips/Locations/Create/{tripId:guid}`  
**API call on POST:** `POST /trips/{tripId}/locations` with `LocationUpsertRequest`

**Full page form (not a sheet panel).**

```html
<article class="panel-card">
  <h1>Add Location</h1>
  <form method="post">
    <div asp-validation-summary="All"></div>
    <label>Name <input required /></label>
    <label>Description <textarea></textarea></label>
    <div class="form-grid">
      <label>Latitude <input type="number" step="any" /></label>
      <label>Longitude <input type="number" step="any" /></label>
    </div>
    <div class="form-grid">
      <label>Start Date <input type="date" /></label>
      <label>End Date <input type="date" /></label>
    </div>
    <footer class="form-actions">
      <button type="submit">Create Location</button>
      <a role="button" class="secondary" href="/Trips/Locations/{tripId}">Cancel</a>
    </footer>
  </form>
</article>
```

On success → redirect to `/Trips/Locations/{tripId}`

---

### 5.8 Edit Location

**File:** `Pages/Trips/Locations/Edit.cshtml` + `EditModel`  
**Auth:** `[Authorize]`  
**Route:** `GET /Trips/Locations/Edit/{tripId:guid}/{id:guid}` | `POST /Trips/Locations/Edit/{tripId:guid}/{id:guid}`  
**API calls:** `GET /trips/{tripId}/locations/{id}` on load; `PUT /trips/{tripId}/locations/{id}` on submit

**Identical structure to Create Location** with:
- Title: "Edit Location"
- Hidden `Input.Id` field
- Submit button: "Save"
- Cancel → `/Trips/Locations/{tripId}`

---

### 5.9 Delete Location

**File:** `Pages/Trips/Locations/Delete.cshtml` + `DeleteModel`  
**Auth:** `[Authorize]`  
**Route:** `GET /Trips/Locations/Delete/{tripId:guid}/{id:guid}` | `POST /Trips/Locations/Delete/{tripId:guid}/{id:guid}`  
**API calls:** `GET /trips/{tripId}/locations/{id}` on load; `DELETE /trips/{tripId}/locations/{id}` on confirm

Same confirmation pattern as Delete Trip:
```html
<article class="panel-card confirm-card">
  <h1>Delete Location</h1>
  <p>Are you sure you want to delete <strong>{location.Name}</strong>?</p>
  <form method="post" class="form-actions">
    <button type="submit" class="contrast">Delete</button>
    <a role="button" class="secondary" href="/Trips/Locations/{tripId}">Cancel</a>
  </form>
</article>
```

---

### 5.10 Login

**File:** `Pages/Account/Login.cshtml` + `LoginModel`  
**Auth:** `[AllowAnonymous]`  
**Route:** `GET /Account/Login?returnUrl={url}`

No rendered UI. `OnGet` immediately issues an OIDC Challenge (`OpenIdConnect`), redirecting the browser to Keycloak. On successful auth, redirects to `returnUrl` (validated as local URL) or `/`.

---

### 5.11 Logout

**File:** `Pages/Account/Logout.cshtml` + `LogoutModel`  
**Route:** `POST /Account/Logout`

No rendered UI. `OnPost` calls `SignOut` on both Cookie and OpenIdConnect schemes, redirecting to `/`. Keycloak receives a logout notification.

**Note:** The logout is triggered from the Profile page's form — see 5.12.

---

### 5.12 Profile

**File:** `Pages/Account/Profile.cshtml` + `ProfileModel`  
**Auth:** `[Authorize]`  
**Route:** `GET /Account/Profile`

```html
<section class="profile-page panel-card">
  <header class="profile-header">
    <div class="profile-avatar">{FirstInitial}</div>
    <h1>{DisplayName}</h1>
    <p>{Email}</p>
  </header>
  <section class="meta-list">
    <small><strong>Account</strong><span>{DisplayName}</span></small>
    <small><strong>Email</strong><span>{Email}</span></small>
  </section>
  <form method="post" class="profile-logout-form">
    <button type="submit" class="profile-logout-button">Log out</button>
  </form>
</section>
```

**`profile-page`:** `max-width: 34rem`, centered, padding `clamp(1.1rem, 4vw, 1.8rem)`, `display: grid; gap: var(--space-lg)`

**Profile avatar:** `5rem × 5rem` circle, gradient `linear-gradient(145deg, #E3AA99, #DC7147)`, white text, font-size `2rem`, font-weight `700`

**Logout button:** full-width, background `#DC7147`, hover → `#CD9F8F`

**Display name resolution order:** `"name"` claim → `Identity.Name` → `ClaimTypes.Name` → `"Camper"`

**Email resolution order:** `"email"` claim → `ClaimTypes.Email` → `"No email available"`

**POST on logout form** → calls `SignOut` (same as Logout page)

---

### 5.13 Register

**File:** `Pages/Account/Register.cshtml` + `RegisterModel`  
**Auth:** `[AllowAnonymous]`  
**Route:** `GET /Account/Register`

`OnGet` immediately issues a `Redirect` to Keycloak's registration endpoint:
```
{keycloakUrl}/realms/camplog/protocol/openid-connect/registrations?client_id=camplog-web&response_type=code&redirect_uri=/
```

No rendered UI (immediate redirect).

---

## 6. Data Models

### 6.1 TripDto (read)

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Trip identifier |
| `Name` | `string` | Required |
| `Description` | `string?` | Optional |
| `Latitude` | `double?` | Optional GPS coordinate |
| `Longitude` | `double?` | Optional GPS coordinate |
| `StartDate` | `DateOnly?` | Optional |
| `EndDate` | `DateOnly?` | Optional |

### 6.2 TripUpsertRequest (create/edit)

Same shape as `TripDto` minus `Id`. `Name` is `[Required]`.

### 6.3 LocationDto (read)

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `Guid` | Location identifier |
| `TripId` | `Guid` | Parent trip |
| `Name` | `string` | Required |
| `Description` | `string?` | Optional |
| `Latitude` | `double?` | Optional GPS coordinate |
| `Longitude` | `double?` | Optional GPS coordinate |
| `StartDate` | `DateOnly?` | Optional |
| `EndDate` | `DateOnly?` | Optional |

### 6.4 LocationUpsertRequest (create/edit)

Same shape as `LocationDto` minus `Id` and `TripId`. `Name` is `[Required]`.

---

## 7. API Contract

Base URL resolved via Aspire service discovery: `https://api`

All endpoints require a valid **Bearer token** (forwarded from the OIDC access token stored in the cookie session). The API enforces per-user ownership — users can only access their own data.

### 7.1 Trips

| Method | Path | Body | Response |
|--------|------|------|----------|
| `GET` | `/trips` | — | `200 TripDto[]` |
| `GET` | `/trips/{id}` | — | `200 TripDto` / `404` / `403` |
| `POST` | `/trips` | `TripUpsertRequest` | `201 TripDto` |
| `PUT` | `/trips/{id}` | `TripUpsertRequest` | `200 TripDto` / `404` / `403` |
| `DELETE` | `/trips/{id}` | — | `204` / `404` / `403` |

### 7.2 Locations

| Method | Path | Body | Response |
|--------|------|------|----------|
| `GET` | `/trips/{tripId}/locations` | — | `200 LocationDto[]` |
| `GET` | `/trips/{tripId}/locations/{id}` | — | `200 LocationDto` / `404` / `403` |
| `POST` | `/trips/{tripId}/locations` | `LocationUpsertRequest` | `201 LocationDto` |
| `PUT` | `/trips/{tripId}/locations/{id}` | `LocationUpsertRequest` | `200 LocationDto` / `404` / `403` |
| `DELETE` | `/trips/{tripId}/locations/{id}` | — | `204` / `404` |

---

## 8. Authentication

- **Provider:** Keycloak
- **Realm:** `camplog`
- **Client ID:** `camplog-web`
- **Flow:** Authorization Code (OIDC)
- **Session:** Cookie (`SameSite=Lax`)
- **Scopes:** `openid profile email`
- **Claims used:** `name`, `email` (via `GetClaimsFromUserInfoEndpoint = true`)
- **Token forwarding:** Pages fetch the `access_token` from the cookie session and include it as `Authorization: Bearer {token}` on API calls
- **Keycloak URL source:** `configuration["services:keycloak:http:0"]` (Aspire service discovery), fallback `http://localhost:8080`

---

## 9. Navigation / Route Map

```
/                                     Home (anonymous)
/Trips                                Trip list (auth required)
/Trips/Create                         Create trip (HTMX partial)
/Trips/Edit/{id}                      Edit trip (HTMX partial)
/Trips/Delete/{id}                    Delete trip confirm (full page)
/Trips/Locations/{tripId}             Locations list (auth required)
/Trips/Locations/Create/{tripId}      Create location (full page)
/Trips/Locations/Edit/{tripId}/{id}   Edit location (full page)
/Trips/Locations/Delete/{tripId}/{id} Delete location confirm (full page)
/Account/Login?returnUrl={url}        Triggers OIDC challenge
/Account/Logout                       POST only — signs out
/Account/Profile                      User info + logout button (auth required)
/Account/Register                     Redirects to Keycloak registration
/Privacy                              Static privacy page
/Error                                Error page (production only)
```

---

## 10. CSS Component Classes Reference

| Class | Description |
|-------|-------------|
| `app-topbar` | Sticky header bar |
| `app-topbar-inner` | Flex container inside topbar |
| `brand-link` | "CampLog" logo link |
| `user-avatar` | Circular user initial link |
| `login-icon-link` | Circular login icon link |
| `site-main` | Main content area |
| `app-tabbar` | Bottom (mobile) / left (desktop) navigation |
| `tab-link` | Individual tab item |
| `tab-link.is-active` | Active tab highlight |
| `tab-icon` | Emoji icon inside tab |
| `sheet-overlay` | Dark backdrop behind sheet |
| `sheet-panel` | Bottom sheet container |
| `sheet-handle` | Visual drag handle pill |
| `sheet-header` | Sheet title + close button row |
| `sheet-close` | ✕ close button |
| `sheet-content` | Scrollable content inside sheet |
| `sheet-open` | Body class applied when sheet is visible |
| `hero-card` | Landing page hero container |
| `hero-kicker` | Small uppercase label above h1 |
| `hero-metrics` | Horizontal pill list |
| `panel-card` | General purpose card with border/shadow |
| `confirm-card` | Centered narrow confirmation card |
| `page-header` | Section heading area (title, subtitle, count badge) |
| `page-meta` | Small amber pill count badge |
| `inline-actions` | Horizontal flex button group |
| `form-actions` | Same as inline-actions (form context) |
| `form-grid` | 1-col mobile / 2-col desktop form field grid |
| `sheet-form` | Grid form inside sheet panel |
| `trip-list` | Grid of trip cards |
| `trip-item` | Single trip card |
| `trip-main-link` | Primary clickable area of trip card |
| `trip-avatar` | Trip initial avatar square |
| `trip-card-copy` | Text content area of trip card |
| `trip-date-chip` | Amber pill date badge |
| `trip-snippet` | Truncated description line |
| `trip-chevron` | › arrow at end of trip card |
| `trip-card-actions` | Edit/Delete links at bottom of trip card |
| `muted-action` | Low-emphasis text link |
| `fab-add-trip` | Fixed circular + button |
| `trip-empty-state` | Empty trips placeholder |
| `location-list` | Grid of location cards |
| `location-item` | Single location card |
| `empty-state` | Dashed border empty placeholder |
| `meta-list` | Key/value row list |
| `profile-page` | Profile page container |
| `profile-header` | Centered avatar + name + email |
| `profile-avatar` | Large circular avatar |
| `profile-logout-form` | Full-width logout form |
| `profile-logout-button` | Terracotta full-width logout button |

---

## 11. JavaScript Behavior

All JS is inline in `_Layout.cshtml`. No external JS files beyond HTMX and legacy Bootstrap libs (unused).

### Sheet Panel Controller

One IIFE manages the sheet:

```
openSheet()  — removes hidden from overlay/panel, adds body.sheet-open after rAF
closeSheet() — removes body.sheet-open, hides overlay/panel + clears container after 220ms
```

**Event listeners:**
- `overlay click` → `closeSheet()`
- `document click` on `[data-close-sheet]` → `closeSheet()`
- `document keydown` Escape (when `sheet-open` on body) → `closeSheet()`
- `body htmx:afterSwap` on `#trip-form-container` (non-empty) → `openSheet()`

---

## 12. Responsive Breakpoint

**Single breakpoint: `700px`**

| Feature | < 700px (mobile) | ≥ 700px (desktop) |
|---------|------------------|-------------------|
| Tab bar | Fixed bottom, horizontal 3-col | Fixed left sidebar, vertical |
| Tab bar size | `full width × 68px` | `86px × calc(100vh - 56px)` |
| Main content padding | Bottom clears tab bar | Left clears sidebar |
| Trip list | 1 column | 2 columns |
| Location list | 1 column | 2 columns |
| Form grid | 1 column | 2 columns |
| FAB position | `bottom: 80px` | `bottom: 24px` |

---

## 13. Error Handling

- **API errors on forms:** `ModelState.AddModelError(string.Empty, "Unable to create/update trip/location.")` — displayed in `asp-validation-summary="All"`
- **404 from API:** Page returns `NotFound()` result
- **Production error page:** `/Error` (standard Razor Pages error page)
- **HTMX request errors:** Not explicitly handled — browser default behavior

---

## 14. Accessibility Notes

- Tab bar has `aria-label="Primary"` and `aria-current="page"` on active item
- Sheet panel has `role="dialog"`, `aria-modal="true"`, `aria-labelledby="sheet-title"`
- Sheet close button has `aria-label="Close"`
- User avatar link has `aria-label="{displayName}"`
- Login icon link has `aria-label="Login"`
- Trip avatar span has `aria-hidden="true"` (decorative)
- Chevron span has `aria-hidden="true"`
- Profile avatar has `aria-hidden="true"`
