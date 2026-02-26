using Microsoft.AspNetCore.Http.HttpResults;
using UTB.Minute.Contracts; 

var builder = WebApplication.CreateBuilder(args);
// This line is Aspire magic - it hooks up basic telemetry and health checks
builder.AddServiceDefaults(); 
var app = builder.Build();
app.MapDefaultEndpoints(); // More Aspire magic for health checks

var fakeMeals = new List<MealDto>
{        
    new MealDto { Id = 1, Name = "Svíčková na smetaně", Price = 95.50m, IsActive = true },        
    new MealDto { Id = 2, Name = "Smažený sýr", Price = 85.00m, IsActive = true }
};


// YOUR FIRST ENDPOINT
app.MapGet("/api/meals", () =>
{ 
    return TypedResults.Ok(fakeMeals);  
});

app.MapPost("/api/meals", (MealDto newMeal) =>
{
    newMeal.Id = fakeMeals.Max(m => m.Id) + 1;
    fakeMeals.Add(newMeal);
    return TypedResults.Created($"api/meals/{newMeal.Id}",newMeal);
});
app.Run();