using System.Net;
using System.Net.Http.Json;
using UTB.Minute.Contracts; // Make sure this points to your MealDto!
using Xunit;

namespace UTB.Minute.WebApi.Tests;

[Collection("Cantine Collection")]
public class MealsTests(CantineTestFixture fixture)
{
    [Fact]
    public async Task GetMeals_ShouldReturnOk_AndListOfMeals()
    {
        // 1. Act (Send the request)
        var response = await fixture.HttpClient.GetAsync("/meals",TestContext.Current.CancellationToken);

        // 2. Assert Status Code
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // 3. Assert Data (Did we actually get the seeded meals?)
        var meals = await response.Content.ReadFromJsonAsync<MealDto[]>(TestContext.Current.CancellationToken);
        
        Assert.NotNull(meals);
        Assert.True(meals.Length >= 2); // Because we seeded 2 meals in the Fixture!
        Assert.Contains(meals, m => m.Name == "Kuřecí řízek");
    }
}