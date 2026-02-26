using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UTB.Minute.Contracts;

namespace UTB.Minute.WebApi.Tests;

// This factory boots up a fake version of your API in memory!
public class MealsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MealsTests(WebApplicationFactory<Program> factory)
    {
        // Creates a fake "browser" to send requests to our in-memory API
        _client = factory.CreateClient();
    }

    [Fact] // This tells xUnit that this method is an automated test
    public async Task GetMeals_ShouldReturnOk_AndListOfMeals()
    {
        // 1. ACT: Send a GET request to our endpoint
        var response = await _client.GetAsync("/api/meals");

        // 2. ASSERT: Did we get a 200 OK status back?
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // 3. ASSERT: Can we read the JSON as a list of MealDtos?
        var meals = await response.Content.ReadFromJsonAsync<List<MealDto>>();
        Assert.NotNull(meals); // Make sure it isn't empty
        Assert.True(meals.Count >= 2); // We know our fake database has at least 2 items
    }
}