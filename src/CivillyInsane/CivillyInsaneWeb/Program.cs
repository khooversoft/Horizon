using CivillyInsaneWeb.Components;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MudBlazor.Services;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

var builder = WebApplication.CreateBuilder(args);

var datalakeOption = builder.Configuration
    .GetSection("Datalake")
    .Get<DatalakeOption>().NotNull("DatalakeOption not in appsettings.json")
    .Action(x => x.Validate().ThrowOnError());

var graphScopes = builder.Configuration
    .GetSection("MicrosoftGraph:Scopes")
    .Get<string[]>()
    ?? ["https://graph.microsoft.com/User.ReadWrite"];

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi(graphScopes)
    .AddInMemoryTokenCaches();

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Events.OnSignedOutCallbackRedirect = context =>
    {
        context.Response.Redirect(context.Request.PathBase + "/");
        context.HandleResponse();
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRazorPages().AddMicrosoftIdentityUI();
builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();
builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// MudBlazor services
builder.Services.AddMudServices();

// Site services
//builder.Services.AddDatalakeFileStore(datalakeOption);
//builder.Services.AddCivillyInsaneServices();

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
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapControllers();
app.MapRazorPages();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
