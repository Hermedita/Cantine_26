using System.Net;
using UTB.Minute.AdminClient;
using UTB.Minute.AdminClient.Components;


var builder = WebApplication.CreateBuilder(args);

/*
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https://localhost:5156");
});*/

builder.Services.AddHttpClient<CanteenService>(client => client.BaseAddress = new Uri("https://web-api"));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
    

app.Run();