# SKILL: Safe Razor Web Project Bootstrap by Cloning

## When to use
- You need to start a new Razor Pages frontend project quickly while preserving existing behavior.
- You already have a validated UI project with HTMX/Pico integration.

## Steps
1. Copy the current web project folder to a new project folder.
2. Remove `bin/` and `obj/` from the copied project.
3. Rename the `.csproj` to the new project name.
4. Add the new project path to `CampLog.slnx`.
5. Run `dotnet build CampLog.slnx` to verify compile safety.

## CampLog paths
- Source template: `CampLog.Web\`
- New project: `CampLog.Web2\`
- Solution file: `CampLog.slnx`

## Guardrails
- Do not change AppHost wiring unless explicitly requested.
- Do not introduce net-new page behavior without Wedge Playwright specs.
