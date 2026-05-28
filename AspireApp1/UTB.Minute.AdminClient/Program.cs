using System.Net;
using UTB.Minute.AdminClient;
using UTB.Minute.AdminClient.Components;


var builder = WebApplication.CreateBuilder(args);

/*
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https://localhost:5156");
});*/

builder.AddServiceDefaults();

builder.Services.AddHttpClient<CanteenService>(client => client.BaseAddress = new Uri("https://web-api"));

// Add services to the container.
builder.Services.AddRazorComponents()
<<<<<<< Updated upstream
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
    
=======
                .AddInteractiveServerComponents();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = "http://localhost:8080/realms/utb-minute";

    options.ClientId = "utb-minute-canteenclient";
    options.ClientSecret = "z7JfYEWdT338HQCJozUfFRNumj1zrqHi";
    options.ResponseType = OpenIdConnectResponseType.Code;

    options.RequireHttpsMetadata = false;
    options.SaveTokens = true;

    options.CallbackPath = "/signin-oidc";
    options.SignedOutCallbackPath = "/signout-callback-oidc";

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("offline_access");

    options.TokenValidationParameters.NameClaimType = "preferred_username";
    options.TokenValidationParameters.RoleClaimType = "roles";

    options.TokenValidationParameters.ValidateIssuer = false;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("admin-admin"));

    options.AddPolicy("RequireCook", policy => policy.RequireRole("canteen-admin"));
});

builder.Services.AddOpenIdConnectAccessTokenManagement();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpClient("", client =>
{
    client.BaseAddress = new Uri("http://web-api");
});

builder.Services.AddHttpClient("PublicApiClient", client =>
{
    client.BaseAddress = new Uri("http://web-api");
});

builder.Services.AddHttpClient("SecureApiClient", client =>
{
    client.BaseAddress = new Uri("http://web-api");
})
.AddUserAccessTokenHandler();

builder.Services.AddScoped<CanteenService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/login", async (HttpContext ctx, string? returnUrl) =>
{
    string redirectUri = string.IsNullOrWhiteSpace(returnUrl) || !Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)
        ? "/"
        : returnUrl;
    await ctx.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = redirectUri
    });
});

// Endpoint pro odhlášení přes POST formulář
app.MapPost("/logout", async (HttpContext ctx) =>
{
    string? idToken = await ctx.GetTokenAsync("id_token");

    await ctx.RevokeRefreshTokenAsync();

    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/",
        Parameters = { { "id_token_hint", idToken ?? string.Empty } }
    });
});
>>>>>>> Stashed changes

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();