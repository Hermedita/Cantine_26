using Microsoft.EntityFrameworkCore;
using UTB.Minute.Db;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorPages();
builder.AddSqlServerDbContext<MealDbContext>("database"); //changed from DbContext to MealDbContext

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.MapPost("/reset-db", async (MealDbContext context) =>  //changed from DbContext to MealDbContext
{
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();
    await context.SaveChangesAsync();
});

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MealDbContext>();

    await context.Database.EnsureCreatedAsync();

    if (!context.Meals.Any())
    {
        var rizek = new Meal { Name = "Kuřecí řízek", Price = 135, IsActive = true, Description = "Smažený řízek" };
        var smazak = new Meal { Name = "Smažák", Price = 120, IsActive = true, Description = "Sýr" };

        var rizekMenu = new Menu { Meal = rizek, MenuDate = new DateOnly(2026, 4, 6), Portions = 50 };
        var smazakMenu = new Menu { Meal = smazak, MenuDate = new DateOnly(2026, 4, 7), Portions = 0 };

        context.Meals.AddRange(rizek, smazak);
        context.MenuItems.AddRange(rizekMenu, smazakMenu);

        await context.SaveChangesAsync();
    }
}

app.Run();