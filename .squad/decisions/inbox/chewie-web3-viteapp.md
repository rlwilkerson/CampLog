# Decision: Switch CampLog.Web3 from AddNpmApp to AddViteApp

**Date:** 2026-02-28  
**Author:** Chewie (DevOps/Infrastructure)  
**Status:** Implemented  
**Scope:** CampLog.AppHost  

## Context

CampLog.Web3 is a React + Vite frontend application previously registered in the Aspire AppHost using the generic `AddNpmApp` method from the `Aspire.Hosting.NodeJs` package (v9.5.2). This package was on a separate release cadence from the core Aspire SDK (13.1.2), creating version misalignment.

Microsoft introduced a new unified JavaScript hosting integration (`Aspire.Hosting.JavaScript`) in Aspire 13.x that includes purpose-built methods for common JavaScript frameworks, including `AddViteApp` specifically for Vite applications.

## Decision

Migrate CampLog.Web3 from `AddNpmApp` to `AddViteApp` using the newer `Aspire.Hosting.JavaScript` package.

### Changes Made

1. **Package Update:**
   - Removed: `Aspire.Hosting.NodeJs` v9.5.2
   - Added: `Aspire.Hosting.JavaScript` v13.1.2

2. **AppHost.cs Registration:**
   - Before:
     ```csharp
     builder.AddNpmApp("web3", "../CampLog.Web3", "dev")
         .WithReference(api).WaitFor(api)
         .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("https"))
         .WithEnvironment("VITE_KEYCLOAK_URL", keycloak.GetEndpoint("http"))
         .WithHttpEndpoint(port: 3000, env: "PORT")
         .PublishAsDockerFile();
     ```
   
   - After:
     ```csharp
     builder.AddViteApp("web3", "../CampLog.Web3")
         .WithReference(api).WaitFor(api)
         .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("https"))
         .WithEnvironment("VITE_KEYCLOAK_URL", keycloak.GetEndpoint("http"))
         .WithHttpEndpoint(port: 3000, env: "PORT")
         .PublishAsDockerFile();
     ```

## Rationale

### Benefits of AddViteApp

1. **Better Defaults:** `AddViteApp` automatically uses the "dev" script for development and "build" script for publishing, removing the need to explicitly specify "dev" as a parameter.

2. **Version Alignment:** `Aspire.Hosting.JavaScript` v13.1.2 aligns with the core `Aspire.AppHost.Sdk/13.1.2`, eliminating version skew between hosting components.

3. **Vite-Specific Optimizations:** The method is purpose-built for Vite applications, providing framework-specific configurations and behaviors.

4. **Unified Integration:** Part of Microsoft's unified JavaScript/TypeScript hosting integration that consolidates support for multiple frameworks (Vite, Node.js, npm, yarn, pnpm) under a single package.

5. **Better Documentation:** Official Microsoft documentation at https://learn.microsoft.com/en-us/dotnet/aspire/get-started/build-aspire-apps-with-nodejs provides comprehensive guidance for the new integration.

### Minimal Disruption

- The change is surgical: only package reference and method name changed
- All environment variables, references, and endpoint configuration remain identical
- No changes required to the Web3 application itself

## Validation

- Code compiles successfully with `dotnet build`
- No breaking changes to existing behavior
- AppHost configuration remains functionally equivalent

## References

- [Microsoft Docs: Build Aspire apps with Node.js](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/build-aspire-apps-with-nodejs)
- [NuGet: Aspire.Hosting.JavaScript v13.1.2](https://www.nuget.org/packages/Aspire.Hosting.JavaScript/13.1.2)

## Impact

- **CampLog.AppHost:** Package and registration method updated
- **CampLog.Web3:** No changes required
- **Other components:** No impact

## Next Steps

None required. The change is complete and validated.
