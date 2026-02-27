using CampLog.Api.Data;
using CampLog.Api.Endpoints;
using CampLog.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<CampLogDbContext>("camplogdb");

builder.Services.AddOpenApi();
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakUrl = builder.Configuration["services:keycloak:http:0"]
                          ?? "http://localhost:8080";
        options.Authority = $"{keycloakUrl}/realms/camplog";
        options.Audience = "camplog-web";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            NameClaimType = "preferred_username",
            RoleClaimType = "realm_access.roles"
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapTripEndpoints();
app.MapLocationEndpoints();
app.MapGet("/me", async (IUserService userService, ClaimsPrincipal user, CancellationToken ct) =>
{
    var me = await userService.GetOrCreateAsync(user, ct);
    return Results.Ok(new
    {
        me.Id,
        me.KeycloakId,
        me.Email,
        me.DisplayName,
        me.CreatedAt
    });
}).RequireAuthorization();

app.Run();
