---
name: keycloak-logout-params
description: Ensure OpenID Connect logout requests to Keycloak include required parameters when using post_logout_redirect_uri.
domain: authentication
confidence: high
source: earned
---

## Context
Use this pattern when ASP.NET Core OpenID Connect sign-out is customized and `id_token_hint` is removed (for example, to avoid stale-token issues in local/dev Keycloak).

## Patterns
- In `OnRedirectToIdentityProviderForSignOut`, if `context.ProtocolMessage.IdTokenHint` is cleared, set `context.ProtocolMessage.ClientId = context.Options.ClientId`.
- Keep `post_logout_redirect_uri` behavior unchanged via existing `SignedOutRedirectUri`/auth properties.
- Prefer a minimal event-level fix over broader auth pipeline changes.

## Examples
```csharp
options.Events = new OpenIdConnectEvents
{
    OnRedirectToIdentityProviderForSignOut = context =>
    {
        context.ProtocolMessage.IdTokenHint = null;
        context.ProtocolMessage.ClientId = context.Options.ClientId;
        return Task.CompletedTask;
    }
};
```

## Anti-Patterns
- Clearing `id_token_hint` and assuming the framework will always include `client_id` automatically.
- Changing login scopes or authority settings to solve a sign-out parameter issue.
