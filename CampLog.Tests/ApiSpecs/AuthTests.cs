using CampLog.Tests.Helpers;
using System.Net;

namespace CampLog.Tests;

public class AuthTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Trips_Returns401_WhenNotAuthenticated()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/trips");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_Returns401_WhenNotAuthenticated()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TripLocations_Returns401_WhenNotAuthenticated()
    {
        var client = _factory.CreateClient();
        var tripId = Guid.NewGuid();

        var response = await client.GetAsync($"/trips/{tripId}/locations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
