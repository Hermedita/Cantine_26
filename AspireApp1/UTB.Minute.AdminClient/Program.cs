using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net;
using UTB.Minute.AdminClient;
using UTB.Minute.AdminClient.Components;


var builder = WebApplication.CreateBuilder(args);

IdentityModelEventSource.ShowPII = true;

/*
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https://localhost:5156");
});*/

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
        options.ClientId = "utb-minute-adminclient";
        options.ClientSecret = "z7JfYEWdT338HQCJozUfFRNumj1zrqHi";
        options.ResponseType = OpenIdConnectResponseType.Code;
        // Ensure PKCE is used with S256 method to match Keycloak client configuration
        options.UsePkce = true;
        options.Scope.Add("openid"); // id_token
        options.Scope.Add("offline_access"); // refresh_token
        options.SaveTokens = true;
        options.RequireHttpsMetadata = false; // jen dev
        options.TokenValidationParameters.NameClaimType = "preferred_username";
        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                // Vynutíme, aby redirect_uri posílaná do Keycloaku byla VŽDY https,
                // protože tvůj prohlížeč běží na https://localhost:7076
                if (context.ProtocolMessage.RedirectUri.StartsWith("http://"))
                {
                    context.ProtocolMessage.RedirectUri = context.ProtocolMessage.RedirectUri.Replace("http://", "https://");
                }
                // Force the S256 code_challenge_method so Keycloak accepts the request
                // when the client is configured to require S256 PKCE.
                // Ensure the outgoing protocol message carries the S256 code_challenge_method
                // so Keycloak accepts the request when the client requires S256 PKCE.
                try
                {
                    context.ProtocolMessage.Parameters["code_challenge_method"] = "S256";
                }
                catch
                {
                    // If Parameters collection is not writable in this environment, ignore.
                }
                return Task.CompletedTask;
            }
        };
    }
);

builder.Services.AddOpenIdConnectAccessTokenManagement(options =>
    options.RefreshBeforeExpiration = TimeSpan.FromSeconds(30)
);

builder.Services.AddCascadingAuthenticationState();

// Configure CanteenService backend URL from configuration with a sensible local default.
// If you're running the WebApi locally, set ApiBaseUrl to its https://localhost:<port> (example: https://localhost:5156)
builder.Services.AddUserAccessTokenHttpClient<CanteenService>(
    configureClient: (_, client) =>
    {
        // The WebApi launchSettings exposes HTTPS on port 7279 and HTTP on 5156.
        // Prefer the HTTPS endpoint used by the WebApi project during development.
        var apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7279";
        client.BaseAddress = new Uri(apiBase);
        // Increase timeout so transient delays are less likely to surface as immediate 30s Polly timeouts
        client.Timeout = TimeSpan.FromSeconds(100);
    });

var app = builder.Build();

app.MapDefaultEndpoints();

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

    await ctx.RevokeRefreshTokenAsync();

    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/",
        Parameters = { { "id_token_hint", idToken ?? string.Empty } }
    });
});

app.Run();