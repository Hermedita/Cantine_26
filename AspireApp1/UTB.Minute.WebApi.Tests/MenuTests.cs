using System.Formats.Asn1;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using UTB.Minute.Contracts;
using UTB.Minute.Db;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

[Collection("Cantine Collection")]
public class MenuTests(CantineTestFixture fixture)
{
    [Fact]
    public async Task GetAllMenus_ReturnsOK_WherePortionBiggerThanZero()
    {
        var response = await fixture.HttpClient.GetAsync("/menus",TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK,response.StatusCode);

        MenuDto[]? menuDto = await response.Content.ReadFromJsonAsync<MenuDto[]>(TestContext.Current.CancellationToken);

        Assert.NotNull(menuDto);
        Assert.True(menuDto.Length >= 1);
        Assert.All(menuDto, m => Assert.True(m.Portions > 0));
        Assert.DoesNotContain(menuDto, m => m.MealName == "Smažák");
    }

    [Fact]
    public async Task CreateNewMenu_ReturnsBadRequest_WhenPortionAreZeroOrLess()
    {
        using var context = fixture.CreateContext();
        var existingMeal = context.Meals.First();

        MenuRequestDto newRequestDto = new MenuRequestDto() {Date = new DateOnly(2026, 4, 6),Portions = 0, MealId = existingMeal.MealId};

        var response = await fixture.HttpClient.PostAsJsonAsync("/menus",newRequestDto,TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
    }

    [Fact]
    public async Task CreateNewMenu_ReturnsBadRequest_WhenMealIdDoesNotExist()
    {
        MenuRequestDto newRequestDto = new MenuRequestDto() {Date = new DateOnly(2026, 4, 6),Portions = 100, MealId = 999999999};

        var response = await fixture.HttpClient.PostAsJsonAsync("/menus",newRequestDto,TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
    }
    [Fact]
    public async Task CreateNewMenu_ReturnsBadRequest_WhenMealIsAlreadyScheduledForTheSameDame()
    {
        using var context = fixture.CreateContext();
        var existingMeal = context.Meals.First();
        
        MenuRequestDto menuRequestDto = new MenuRequestDto() {Date = new DateOnly(2026, 4, 6),Portions = 100, MealId = existingMeal.MealId};

        var response = await fixture.HttpClient.PostAsJsonAsync("/menus",menuRequestDto,TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
    }
    [Fact]
    public async Task CreateNewMenu_ReturnsCreated_WhenMenuIsCreatedAndPersistsMenu()
    {
        using var context = fixture.CreateContext();
        var existingMeal = context.Meals.First();

        MenuRequestDto menuRequestDto = new MenuRequestDto() { 
            Date = new DateOnly(2026, 10, 10), 
            Portions = 100, 
            MealId = existingMeal.MealId 
        };

        var response = await fixture.HttpClient.PostAsJsonAsync("/menus",menuRequestDto,TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created,response.StatusCode);

        MenuDto? menuDto = await response.Content.ReadFromJsonAsync<MenuDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(menuDto);
        Assert.Equal(menuRequestDto.MealId,menuDto.MealId);

        using var verifyContext = fixture.CreateContext(); 

        Menu? menu = await verifyContext.MenuItems.FindAsync([menuDto.Id],TestContext.Current.CancellationToken);
        Assert.NotNull(menu);
        Assert.Equal(menuRequestDto.MealId,menu.MealId);
    }

    [Fact]
    public async Task UpdateMenu_ReturnsNotFound_WhenMenuDoesNotExist()
    {
        using var context = fixture.CreateContext();
        var existingMeal = context.Meals.First();

        MenuRequestDto menuRequestDto = new MenuRequestDto() { 
            Date = new DateOnly(2026, 10, 10), 
            Portions = 100, 
            MealId = existingMeal.MealId
        };

        var response = await fixture.HttpClient.PutAsJsonAsync("/menus/999999",menuRequestDto,TestContext.Current.CancellationToken); 

        Assert.Equal(HttpStatusCode.NotFound,response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateMenu_ReturnsBadRequest_WhenMealIdDoesNotExist()
    {
        using var context = fixture.CreateContext();
        var realMenu = context.MenuItems.First();

        MenuRequestDto menuRequestDto = new MenuRequestDto() { 
            Date = new DateOnly(2026, 10, 10), 
            Portions = 100, 
            MealId = 9999999
        };

        var response = await fixture.HttpClient.PutAsJsonAsync($"/menus/{realMenu.MenuId}",menuRequestDto,TestContext.Current.CancellationToken); 

        Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
    }

    [Fact]
    public async Task UpdateMenu_ReturnsNoContent_AndUpdatesMenu()
    {
        using var context = fixture.CreateContext();
        var realMenu = context.MenuItems.First();

        MenuRequestDto menuRequestDto = new MenuRequestDto() { 
            Date = new DateOnly(2026, 11, 11), 
            Portions = 50, 
            MealId = realMenu.MealId
        };

        var response = await fixture.HttpClient.PutAsJsonAsync($"/menus/{realMenu.MenuId}",menuRequestDto,TestContext.Current.CancellationToken); 
        Assert.Equal(HttpStatusCode.NoContent,response.StatusCode);

        using var verifyContext = fixture.CreateContext();
        var updatedMenu = await verifyContext.MenuItems.FindAsync([realMenu.MenuId], TestContext.Current.CancellationToken);

        Assert.NotNull(updatedMenu);
        Assert.Equal(new DateOnly(2026, 11, 11), updatedMenu.MenuDate);
    }

    [Fact]
    public async Task DeleteMenu_ReturnsNotFound_WhenMenuDoesNotExist()
    {
        var response = await fixture.HttpClient.DeleteAsync("/menus/999999", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMenu_ReturnsNoContent_AndDeletesMenu()
    {
        int menuIdToDelete;
    
        using (var setupContext = fixture.CreateContext())
        {
            var existingMeal = setupContext.Meals.First();
            var sacrificialMenu = new Menu()
            {
                MealId = existingMeal.MealId,
                MenuDate = new DateOnly(2026, 12, 31),
                Portions = 5
            };
        
            setupContext.MenuItems.Add(sacrificialMenu);
            await setupContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        
            menuIdToDelete = sacrificialMenu.MenuId; 
        }

        var response = await fixture.HttpClient.DeleteAsync($"/menus/{menuIdToDelete}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var verifyContext = fixture.CreateContext();
        var deletedMenu = await verifyContext.MenuItems.FindAsync([menuIdToDelete], TestContext.Current.CancellationToken);

        Assert.Null(deletedMenu); 
    }
}
