using System.Net;
using System.Net.Http.Json;
using UTB.Minute.Contracts;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

[Collection("Cantine Collection")]
public class MealsTests(CantineTestFixture fixture)
{
    [Fact]
    public async Task GetMeals_ShouldReturnOk_AndListOfMeals()
    {
        var response = await fixture.HttpClient.GetAsync("/meals",TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var meals = await response.Content.ReadFromJsonAsync<MealDto[]>(TestContext.Current.CancellationToken);
        
        Assert.NotNull(meals);
        Assert.True(meals.Length >= 2);
        Assert.Contains(meals, m => m.Name == "Kuřecí řízek");
    }
}