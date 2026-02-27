using CampLog.Api;
using CampLog.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CampLog.Tests.Helpers;

public class TestWebApplicationFactory : WebApplicationFactory<ApiAssemblyMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Provide a placeholder connection string so Aspire's AddNpgsqlDbContext doesn't throw
        builder.UseSetting("ConnectionStrings:camplogdb",
            "Host=localhost;Database=testdb;Username=test;Password=test");

        // Disable HTTPS redirect so test client (http://localhost) gets direct 401, not 307
        builder.UseSetting("https_port", "");

        builder.ConfigureServices(services =>
        {
            // Replace Npgsql DbContext with in-memory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CampLogDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<CampLogDbContext>(opts =>
                opts.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));
        });

        builder.UseEnvironment("Test");
    }
}
