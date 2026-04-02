using UTB.Minute.Db;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); 

builder.AddSqlServerDbContext<MealDbContext>("database");

var app = builder.Build();

app.MapDefaultEndpoints();


app.MapGet("/meals", WebAPI.PrintMeals);
app.MapPost("/meals", WebAPI.CreateNewMeal);
app.MapPut("/meals/{id}", WebAPI.UpdateMeal);
app.MapPatch("/meals/{id}", WebAPI.DeactivateMeal);

app.MapGet("/menus", WebAPI.PrintMenus);
app.MapPost("/menus", WebAPI.CreateNewMenu);
app.MapPut("/menus", () => "Menu");
app.MapDelete("/menus/{id}", () => "Menu");

app.MapGet("/orders/active", () => "Orders");
app.MapGet("/orders/menu", () => "Orders");
app.MapPost("/orders", () => "Orders");
app.MapPut("/orders/{id}/status", () => "Orders");

app.Run();