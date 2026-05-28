using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using UTB.Minute.Db;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); 

builder.AddSqlServerDbContext<MealDbContext>("database");

<<<<<<< Updated upstream
=======
builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(
        serviceName: "keycloak",
        realm: "utb-minute",
        options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = true,
                NameClaimType = "preferred_username",
                RoleClaimType = "roles"
            };

            options.RequireHttpsMetadata = false;

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    if (context.Principal?.Identity is ClaimsIdentity claimsIdentity)
                    {
                        var rolesClaim = context.Principal.FindFirst("roles")?.Value;

                        foreach (var claim in context.Principal.Claims)
                        {
                            if (claim.Value.Contains("admin-admin"))
                            {
                                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "admin-admin"));
                            }
                            if (claim.Value.Contains("canteen-admin"))
                            {
                                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "canteen-admin"));
                            }
                        }
                    }
                    return Task.CompletedTask;
                }
            };
        });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("admin-admin"));

    options.AddPolicy("RequireCook", policy => policy.RequireRole("canteen-admin"));
});

builder.Services.AddControllers();

>>>>>>> Stashed changes
builder.Services.AddSingleton<SseNotificationService>();

var app = builder.Build();

app.MapDefaultEndpoints();


app.MapGet("/meals", WebAPI.PrintMeals);
app.MapGet("/meals/{id}", WebAPI.GetMeal);
app.MapPost("/meals", WebAPI.CreateNewMeal).RequireAuthorization("RequireAdmin");
app.MapPut("/meals/{id}", WebAPI.UpdateMeal).RequireAuthorization("RequireAdmin");

app.MapGet("/menus", WebAPI.PrintMenus);

app.MapGet("/menus/{id:int}", WebAPI.GetMenu);
app.MapPost("/menus", WebAPI.CreateNewMenu).RequireAuthorization("RequireAdmin");
app.MapPut("/menus/{id}", WebAPI.UpdateMenu).RequireAuthorization("RequireAdmin");
app.MapDelete("/menus/{id}", WebAPI.DeleteMenu).RequireAuthorization("RequireAdmin");

app.MapPatch("/meals/{id}/state", WebAPI.ChangeMealState).RequireAuthorization("RequireCook");
app.MapGet("/orders", WebAPI.PrintOrders).RequireAuthorization("RequireCook");
app.MapPut("/orders/{id}/status", WebAPI.UpdateOrderStatus).RequireAuthorization("RequireCook");

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