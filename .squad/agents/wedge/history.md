# Wedge — Project History

## Core Context
**Project:** CampLog — RV trip tracking application
**Owner:** Rick Wilkerson
**Stack:** xUnit, ASP.NET Core integration testing, Testcontainers (PostgreSQL)

**Testing responsibilities:**
- Unit tests: service layer, data model validation
- Integration tests: API endpoints with real PostgreSQL via Testcontainers
- Edge cases: auth failures, empty trip lists, null GPS, concurrent edits, multi-user isolation
- Quality gate: no merge without test coverage for new features

## Learnings
<!-- Append test patterns, edge cases found, quality gate rules below -->
