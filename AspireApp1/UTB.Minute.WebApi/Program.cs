using UTB.Minute.Db;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); 

builder.AddSqlServerDbContext<MealDbContext>("database");

var app = builder.Build();

app.MapDefaultEndpoints();


app.MapGet("/meals", WebAPI.PrintMeals);
app.MapPost("/meals", WebAPI.CreateNewMeal);
app.MapPut("/meals/{id}", WebAPI.UpdateMeal);
app.MapPatch("/meals/{id}/state", WebAPI.ChangeMealState);

app.MapGet("/menus", WebAPI.PrintMenus);
app.MapPost("/menus", WebAPI.CreateNewMenu);
app.MapPut("/menus/{id}", WebAPI.UpdateMenu);
app.MapDelete("/menus/{id}", WebAPI.DeleteMenu);

app.MapGet("/orders", WebAPI.PrintOrders);
app.MapPost("/orders", WebAPI.CreateNewOrder);
app.MapPut("/orders/{id}/status", WebAPI.UpdateOrderStatus);

app.Run();