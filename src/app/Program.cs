using LowlandTech.TinyTools.App.Components;
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Frontend.Features.Templates;
using LowlandTech.TinyTools.Frontend.Services.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// The page reads through the server, never through a bare HttpClient, so the
// base address is configuration and a missing value fails here rather than at
// the first render.
var apiBaseAddress = builder.Configuration["Tinytools:ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseAddress))
{
    throw new InvalidOperationException(
        "Set Tinytools:ApiBaseUrl (for example http://localhost:5199) so the app can reach the template API.");
}

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseAddress, UriKind.Absolute) });
builder.Services.AddScoped<ITemplateLibrary, HttpTemplateLibrary>();
builder.Services.AddScoped<TemplateEffects>();
builder.Services.AddScoped<TemplateSessionStore>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
