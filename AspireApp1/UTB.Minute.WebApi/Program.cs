using UTB.Minute.Db;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); 

builder.AddSqlServerDbContext<MealDbContext>("database");

builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(
        serviceName: "keycloak",
        realm: "utb-minute",
        options =>
        {
            options.Audience = "utb-minute-webapi";
            options.RequireHttpsMetadata = false; // jen pro dev
        }
    );

builder.Services.AddAuthorization();

builder.Services.AddSingleton<SseNotificationService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/meals", WebAPI.PrintMeals).RequireAuthorization(pb => pb.RequireRole("admin-admin"));
app.MapGet("/meals/{id}", WebAPI.GetMeal);
app.MapPost("/meals", WebAPI.CreateNewMeal);
app.MapPut("/meals/{id}", WebAPI.UpdateMeal);
app.MapPatch("/meals/{id}/state", WebAPI.ChangeMealState);

app.MapGet("/menus", WebAPI.PrintMenus);
app.MapPost("/menus", WebAPI.CreateNewMenu);
app.MapPut("/menus/{id}", WebAPI.UpdateMenu);
app.MapDelete("/menus/{id}", WebAPI.DeleteMenu);
app.MapGet("/menus/{id:int}", WebAPI.GetMenu);

app.MapGet("/orders", WebAPI.PrintOrders);
app.MapPost("/orders", WebAPI.CreateNewOrder);
app.MapPut("/orders/{id}/status", WebAPI.UpdateOrderStatus);

app.MapGet("/api/orders/sse", async (HttpContext context, SseNotificationService sseService, CancellationToken ct) =>
{
    context.Response.Headers.Add("Content-Type", "text/event-stream");
    context.Response.Headers.Add("Cache-Control", "no-cache");
    context.Response.Headers.Add("Connection", "keep-alive");

    using var writer = new StreamWriter(context.Response.Body);
    sseService.AddClient(writer);

    try
    {
        await Task.Delay(Timeout.Infinite, ct);
    }
    catch (TaskCanceledException)
    {
        // Klient se odpojil
    }
});
app.MapPost("/api/orders/notify-change", async (OrderUpdateMessage model, SseNotificationService sseService) =>
{
    await sseService.BroadcastOrderUpdateAsync(model);
    return Results.Ok();
});

app.Run();