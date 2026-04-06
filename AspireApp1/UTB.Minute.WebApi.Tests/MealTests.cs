using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using UTB.Minute.Contracts;
using UTB.Minute.Db;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

[Collection("Cantine Collection")]
public class MealsTests(CantineTestFixture fixture)
{
    [Fact]
    public async Task GetAllMeals_ReturnsOk_AndListOfMeals()
    {
        var response = await fixture.HttpClient.GetAsync("/meals",TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        MealDto[]? mealDto = await response.Content.ReadFromJsonAsync<MealDto[]>(TestContext.Current.CancellationToken);
        
        Assert.NotNull(mealDto);
        Assert.True(mealDto.Length >= 2);
        Assert.Contains(mealDto, m => m.Name == "Kuřecí řízek");
        Assert.Contains(mealDto, m => m.Name == "Smažák");
    }

    [Fact]
    public async Task CreateMeal_ReturnsCreatedAndPersistsMeal()
    {
        var mealRequestDto = new MealRequestDto()
        {
           Name = "Hovězí guláš",
           Price = 100,
           Description = "Klasický český guláš s houskovým knedlíkem"  
        };

        var response = await fixture.HttpClient.PostAsJsonAsync("/meals",mealRequestDto,TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created,response.StatusCode);

        MealDto? mealDto = await response.Content.ReadFromJsonAsync<MealDto>(TestContext.Current.CancellationToken);

        Assert.NotNull(mealDto);
        Assert.Equal(mealRequestDto.Name,mealDto.Name);
        
        using var context = fixture.CreateContext(); 

        Meal? meal = await context.Meals.FindAsync([mealDto.Id],TestContext.Current.CancellationToken);

        Assert.NotNull(meal);
        Assert.Equal(mealRequestDto.Name,meal.Name);

    }

    [Fact]
    public async Task UpdateMeal_ReturnsNotFound_WhenMealDoesNotExist()
    {
        
        var mealRequest = new MealRequestDto()
        {
           Name = "Hovězí guláš",
           Price = 100,
           Description = "Klasický český guláš s houskovým knedlíkem"  
        };

        var response = await fixture.HttpClient.PutAsJsonAsync("/meals/99999",mealRequest,TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound,response.StatusCode);
    }

    [Fact]
    public async Task UpdateMeal_ReturnsNoContent_AndUpdatesMeal()
    {
        var meal = new Meal{Name="Jídlo",Price=99,Description="Popis"};

        using (var context = fixture.CreateContext())
        {
            context.Meals.Add(meal);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var mealRequest = new MealRequestDto()
        {
           Name = "Hovězí guláš",
           Price = 100,
           Description = "Klasický český guláš s houskovým knedlíkem"  
        };

        var response = await fixture.HttpClient.PutAsJsonAsync($"/meals/{meal.MealId}",mealRequest,TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent,response.StatusCode);

        using (var context = fixture.CreateContext())
        {
            var newMeal = await context.Meals.FindAsync([meal.MealId],TestContext.Current.CancellationToken);

            Assert.NotNull(newMeal);
            Assert.Equal(mealRequest.Name,newMeal.Name);
        }

    }

    [Fact]
    public async Task ChangeMealState_ReturnsNotFound_WhenMealDoesntExists()
    {
        MealStateRequestDto mealStateRequestDto = new() { IsActive = false };

        var response = await fixture.HttpClient.PatchAsJsonAsync("/meals/99999/state", mealStateRequestDto, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangeMealState_ReturnsNoContent_WhenMealChangesState()
    {
        var meal = new Meal{Name="Jídlo",Price=99,Description="Popis",IsActive=true};

        using (var context = fixture.CreateContext())
        {
            context.Meals.Add(meal);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        MealStateRequestDto mealStateRequestDto = new() {IsActive = false};

        var response = await fixture.HttpClient.PatchAsJsonAsync($"/meals/{meal.MealId}/state",mealStateRequestDto,TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent,response.StatusCode);

        using (var context = fixture.CreateContext())
        {
            var newMeal = await context.Meals.FindAsync([meal.MealId],TestContext.Current.CancellationToken);

            Assert.NotNull(newMeal);
            Assert.Equal(meal.Name,newMeal.Name);
            Assert.Equal(mealStateRequestDto.IsActive,newMeal.IsActive);
        }
    }
}