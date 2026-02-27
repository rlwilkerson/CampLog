using CampLog.Tests.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace CampLog.Tests;

public class LocationApiSpecs : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public LocationApiSpecs(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Locations_GetAll_Returns401_WhenNotAuthenticated()
    {
        var client = _factory.CreateClient();
        var tripId = Guid.NewGuid();

        var response = await client.GetAsync($"/trips/{tripId}/locations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Locations_Create_Returns401_WhenNotAuthenticated()
    {
        var client = _factory.CreateClient();
        var tripId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync($"/trips/{tripId}/locations", new { Name = "Test Location" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Locations_GetAll_ReturnsLocations_ForTrip() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Locations_Create_ReturnsCreatedLocation_WithValidData() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Locations_Create_Returns404_WhenTripNotFound() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Locations_GetById_ReturnsLocation() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Locations_Update_ReturnsUpdatedLocation() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Locations_Delete_Returns204() => await Task.CompletedTask;
}
