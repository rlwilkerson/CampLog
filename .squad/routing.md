# Routing Rules

## Signal → Agent Mapping

| Signal | Agent | Notes |
|--------|-------|-------|
| Architecture, scope, design decisions, cross-cutting concerns | Yoda | Lead — always in the loop on big calls |
| API endpoints, controllers, services, repositories | Luke | .NET 10 Web API, EF Core, PostgreSQL |
| Data models, migrations, database schema | Luke | Entity Framework, Npgsql |
| Razor Pages, HTMX interactions, page layouts | Leia | Frontend — UI and UX owner |
| Pico CSS, color theme, mobile responsiveness | Leia | Dusty Summer palette, responsive design |
| React components, hooks, TypeScript types | Lando | React dev — CampLog.Web2 owner |
| React routing, state management, API integration | Lando | TanStack Query, React Router |
| React UI design, animation, accessibility | Lando | Dusty Summer in React, WCAG AA |
| .NET Aspire configuration, orchestration | Chewie | AppHost, service defaults, resources |
| Keycloak setup, OIDC integration, auth flow | Chewie | Identity provider config |
| Docker Compose, environment variables, infra config | Chewie | Dev/prod environment parity |
| Test coverage, unit tests, integration tests | Wedge | xUnit, quality gates |
| Edge cases, validation, error handling review | Wedge | QA lens on any change |
| Session logging, decisions, memory | Scribe | Silent — triggered by coordinator |
| Backlog scanning, issue triage, PR monitoring | Ralph | Active when "Ralph, go" |

## Parallel Fan-Out Triggers

- "Build a feature" → Yoda (arch) + Luke (API) + Leia (UI) + Wedge (tests)
- "Set up auth" → Chewie (Keycloak) + Luke (backend integration) + Leia (login UI)
- "Add a page" → Leia (Razor + HTMX) + Luke (backing API) + Wedge (test coverage)
- "Review" → Yoda + Wedge

## Reviewer Gates

- Wedge reviews all PRs for test coverage before merge
- Yoda reviews architecture changes before implementation begins
