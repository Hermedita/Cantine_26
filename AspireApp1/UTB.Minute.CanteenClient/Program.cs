using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using UTB.Minute.CanteenClient;
using UTB.Minute.CanteenClient.Components;

var builder = WebApplication.CreateBuilder(args);

IdentityModelEventSource.ShowPII = true;

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddKeycloakOpenIdConnect(
        serviceName: "keycloak",
        realm: "utb-minute",
        options =>
        {
            options.ClientId = "utb-minute-canteenclient";
            options.ClientSecret = "DUnhGX8BGoniwxq3htURasayK0Y1m3nT";
            // Ensure the OIDC callback path is the standard path Keycloak expects
            options.CallbackPath = "/signin-oidc";
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.UsePkce = true;
            options.Scope.Add("openid");
            options.Scope.Add("offline_access");
            options.SaveTokens = true;
            options.RequireHttpsMetadata = false; // dev
            options.TokenValidationParameters.NameClaimType = "preferred_username";
            // Map Keycloak realm roles (realm_access.roles) into a claim named 'roles'
            options.ClaimActions.MapJsonKey("roles", "realm_access.roles");
            // Keycloak returns role claims using the standard role claim type URI.
            // Use ClaimTypes.Role so IsInRole/AuthorizeView will find the role claims.
            options.TokenValidationParameters.RoleClaimType = System.Security.Claims.ClaimTypes.Role;
            options.Events = new OpenIdConnectEvents
            {
                OnRedirectToIdentityProvider = context =>
                {
                    if (context.ProtocolMessage.RedirectUri.StartsWith("http://"))
                    {
                        context.ProtocolMessage.RedirectUri = context.ProtocolMessage.RedirectUri.Replace("http://", "https://");
                    }
                    // Log the full auth request URL and parameters so you can inspect redirect_uri and PKCE method
                    try
                    {
                        Console.WriteLine("Auth request URL: " + context.ProtocolMessage.CreateAuthenticationRequestUrl());
                        foreach (var p in context.ProtocolMessage.Parameters)
                        {
                            Console.WriteLine($"param: {p.Key} = {p.Value}");
                        }
                        context.ProtocolMessage.Parameters["code_challenge_method"] = "S256";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("OnRedirectToIdentityProvider logging failed: " + ex.Message);
                    }
                    return Task.CompletedTask;
                }
            };
        }
    );

builder.Services.AddCascadingAuthenticationState();

// Configure CanteenService HttpClient to point to local WebApi HTTPS in development
builder.Services.AddHttpClient<CanteenService>(client =>
{
    var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7279";
    client.BaseAddress = new Uri(apiBase);
    client.Timeout = TimeSpan.FromSeconds(100);
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
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

app.MapGet("/login", async (HttpContext ctx, string? returnUrl) =>
{
    string redirectUri = "/";

    if (!string.IsNullOrWhiteSpace(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
    {
        redirectUri = returnUrl;
    }

    await ctx.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = redirectUri,
        IsPersistent = false
    });
});

app.MapPost("/logout", async (HttpContext ctx) =>
{
    string? idToken = await ctx.GetTokenAsync("id_token");

    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/",
        Parameters = { { "id_token_hint", idToken ?? string.Empty } }
    });
});

app.Run();
