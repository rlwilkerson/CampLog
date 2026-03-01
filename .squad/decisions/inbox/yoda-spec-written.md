# Decision: SPEC.md is Authoritative Architecture Reference

**Date:** 2026-02-28  
**By:** Yoda (Lead / Architect)  
**Status:** Active

## What
Established `docs/SPEC.md` as the single authoritative technical specification for the CampLog project.

## Why
- Centralizes all architectural knowledge in one document
- Enables rapid onboarding of new developers or AI agents
- Provides version-locked tech stack reference (exact package versions)
- Documents all three frontend variants (Web, Web2, Web3) with implementation details
- Captures key decisions with historical context
- Serves as contract between team members on system design

## Contents
The specification covers:
1. Project overview and goals
2. Solution structure (all 7 projects)
3. Complete tech stack with versions
4. Architecture (Aspire orchestration, dependencies, auth flows)
5. Data model (User, Trip, Location with all fields and relationships)
6. API (endpoints, DTOs, authorization rules)
7. Frontend variants (Razor+HTMX, React+MUI) with design systems
8. Identity & auth (Keycloak realm, OIDC flows, test credentials)
9. Infrastructure (Postgres, Keycloak, telemetry, health checks)
10. Development setup (prerequisites, commands, ports)
11. All key architectural decisions from .squad/decisions.md

## Impact
- **Onboarding:** New team members can understand entire system from one document
- **AI Context:** Specification can be fed to AI models for accurate system understanding
- **Documentation Debt:** Eliminates need to explore code to understand architecture
- **Decision Tracking:** Consolidates scattered decisions into cohesive narrative
- **Maintenance:** Single source of truth reduces documentation drift

## Maintenance
- Update SPEC.md when:
  - Major architectural changes occur
  - New projects added to solution
  - Tech stack versions updated
  - Authentication/authorization flow changes
  - New frontend variants added
- Version specification document (currently v1.0)
- Review quarterly or before major releases

## Location
`C:\code\rlwilkerson\CampLog\main\docs\SPEC.md`
