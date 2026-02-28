---
name: "aspire-apphost-add-project-resource"
description: "Add a new .NET project as an Aspire AppHost resource with minimal wiring changes"
domain: "aspire-apphost"
confidence: "high"
source: "manual"
---

## When to use
- A new app/API project already exists in the repo and must be orchestrated by `CampLog.AppHost`.

## Steps
1. Add a `<ProjectReference>` for the new project in `CampLog.AppHost\CampLog.AppHost.csproj`.
2. Register the resource in `CampLog.AppHost\AppHost.cs` using `builder.AddProject<Projects.{ProjectAlias}>("{resourceName}")`.
3. Mirror dependency wiring from sibling resources (e.g., `.WithReference(api).WaitFor(api)` and Keycloak references for web frontends).
4. Validate with:
   - `dotnet build CampLog.slnx`
   - `aspire run --project CampLog.AppHost\CampLog.AppHost.csproj --non-interactive`

## Guardrails
- Keep changes minimal and match existing AppHost style/patterns.
- Do not add unrelated service references.
