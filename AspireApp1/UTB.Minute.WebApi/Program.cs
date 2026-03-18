using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using UTB.Minute.Contracts;
using UTB.Minute.Db;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); 

builder.AddSqlServerDbContext<MealDbContext>("database");

var app = builder.Build();

app.MapDefaultEndpoints();


app.MapGet("/meals", WebAPI.PrintMeals);
app.MapPost("/meals", WebAPI.CreateNewMeal);
app.MapPut("/meals/{id}", WebAPI.UpdateMeal);
app.MapDelete("meals/{id}", WebAPI.DeactivateMeal);

app.MapGet("/menus", () => "Menu");
app.MapPost("/menus", () => "Menu");
app.MapPut("/menus", () => "Menu");
app.MapDelete("/menus/{id}", () => "Menu");

app.MapGet("/orders/active", () => "Orders");
app.MapGet("/orders/menu", () => "Orders");
app.MapPost("/orders", () => "Orders");
app.MapPut("/orders/{id}/status", () => "Orders");

app.Run();

public static class WebAPI
{
    public static async Task<IResult> PrintMeals(MealDbContext db)
    {
        var meals = await db.Meals.ToListAsync();

        var mealDTOs = meals.Select(m => new MealDto
        {
            Id = m.MealId,
            Name = m.Name,
            Price = m.Price,
            Description = m.Description,
            IsActive = m.IsActive
        });

        return TypedResults.Ok(mealDTOs);
    }

    public static async Task<IResult> CreateNewMeal(MealDto newMealDTO,MealDbContext db)
    {

        if (newMealDTO.Name == null || newMealDTO.Price == null){
            return TypedResults.BadRequest("Jídlo musí mít název a cenu!");
        }

        var newMealEntity = new Meal
        {
            Name = newMealDTO.Name,
            Price = newMealDTO.Price,
            Description = "Nové jídelko yippee",
            IsActive = true 
        };

        db.Meals.Add(newMealEntity);

        await db.SaveChangesAsync();

        newMealDTO.Id = newMealEntity.MealId;
        newMealDTO.Description = newMealEntity.Description;
        newMealDTO.IsActive = true;

        return TypedResults.Created($"/meals/{newMealEntity.MealId}",newMealDTO);
    }

    public static async Task<IResult> UpdateMeal(int id, MealDto updatedMealDTO, MealDbContext db)
    {
        if (id != updatedMealDTO.Id)
        {
            return TypedResults.BadRequest("ID v URL se neshoduje s ID v těle požadavku");
        }

        if (updatedMealDTO.Name == null || updatedMealDTO.Price == null)
        {
            return TypedResults.BadRequest("Jídlo musí mít název a cenu!");
        }
        var existingMeal = await db.Meals.FindAsync(id);
        if (existingMeal == null)
        {
            return TypedResults.NotFound();
        }

        existingMeal.Name = updatedMealDTO.Name;
        existingMeal.Price = updatedMealDTO.Price;
        existingMeal.Description = updatedMealDTO.Description;
        existingMeal.IsActive = updatedMealDTO.IsActive;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    public static async Task<IResult> DeactivateMeal(int id, MealDbContext db)
    {
        var existingMeal = await db.Meals.FindAsync(id);
        if(existingMeal == null)
        {
            return TypedResults.NotFound();
        }

        existingMeal.IsActive = false;

        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}