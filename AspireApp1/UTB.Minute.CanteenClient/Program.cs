using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using UTB.Minute.CanteenClient;
using UTB.Minute.CanteenClient.Components;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    var keycloakBaseUrl = builder.Configuration["ProductService:LaunchProfile:keycloak"]
                          ?? builder.Configuration["messaging"]
                          ?? "http://localhost:8080";
    var aspireKeycloakUrl = builder.Configuration["ConnectionStrings:keycloak"];

    options.Authority = string.IsNullOrEmpty(aspireKeycloakUrl) ? "http://localhost:8080/realms/utb-minute" : $"{aspireKeycloakUrl}/realms/utb-minute";

    options.RequireHttpsMetadata = false;
    options.MetadataAddress = $"{options.Authority}/.well-known/openid-configuration";

    options.ClientId = "utb-minute-canteenclient";
    options.ClientSecret = "DUnhGX8BGoniwxq3htURasayK0Y1m3nT";
    options.ResponseType = OpenIdConnectResponseType.Code;
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

builder.Services.AddCascadingAuthenticationState();

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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();