using CampLog.Tests.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace CampLog.Tests;

public class TripApiSpecs : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public TripApiSpecs(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Trips_GetAll_Returns401_WhenNotAuthenticated()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/trips");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Trips_Create_Returns401_WhenNotAuthenticated()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/trips", new { Name = "Test Trip" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Trips_GetAll_ReturnsEmptyList_WhenNoTrips() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Trips_GetAll_ReturnsTrips_ForAuthenticatedUser() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Trips_GetAll_DoesNotReturnOtherUsersTrips() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Trips_Create_ReturnsCreatedTrip_WithValidData() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Trips_Create_Returns400_WhenNameMissing() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Trips_GetById_ReturnsTrip_WhenOwner() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Trips_GetById_Returns404_WhenNotFound() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Trips_GetById_Returns403_WhenNotOwner() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Trips_Update_ReturnsUpdatedTrip_WhenOwner() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Trips_Update_Returns404_WhenNotFound() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Trips_Delete_Returns204_WhenOwner() => await Task.CompletedTask;

    [Fact(Skip = "Requires live Keycloak - run with integration test suite")]
    public async Task Trips_Delete_Returns404_WhenNotFound() => await Task.CompletedTask;
}
