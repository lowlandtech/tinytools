using System.Reflection;
using LowlandTech.Core.Plugins;
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Frontend.Features.Templates;
using LowlandTech.TinyTools.Frontend.Services.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LowlandTech.TinyTools.Frontend;

/// <summary>
/// The Templates surface as an application plugin.
/// <para>
/// A host that composes this package gets the page, the store and the effects
/// from one place. The page injects its store and its effects, so a host that
/// mounts the page without this plugin holds a page it cannot construct - the
/// failure a published client reported until this type existed.
/// </para>
/// </summary>
public sealed class TinyToolsFrontendPlugin : IAppPlugin
{
    /// <summary>The configuration key that names the template API the client calls.</summary>
    public const string ApiBaseUrlKey = "Tinytools:ApiBaseUrl";

    /// <summary>
    /// The identity of the plugin. The version is read from this assembly, so the
    /// descriptor and the package that carries it state the same build.
    /// </summary>
    public PluginDescriptor Descriptor { get; } = new()
    {
        Id = "tinytools",
        Name = "Tinytools Templates",
        Version = typeof(TinyToolsFrontendPlugin).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            .Split('+')[0] ?? "0.0.0",
        Description = "The Templates surface of the Tinytools application.",
    };

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        // The host may name a distinct API origin. Otherwise, the browser host
        // supplies its current origin through NavigationManager.
        var apiBaseAddress = configuration?[ApiBaseUrlKey];
        services.TryAddScoped(provider => new HttpClient
        {
            BaseAddress = new Uri(provider.GetRequiredService<NavigationManager>().BaseUri, UriKind.Absolute)
        });
        services.AddScoped<ITemplateLibrary>(provider =>
        {
            var client = provider.GetRequiredService<HttpClient>();
            if (!string.IsNullOrWhiteSpace(apiBaseAddress))
                client.BaseAddress = new Uri(apiBaseAddress, UriKind.Absolute);
            else if (client.BaseAddress is null)
                client.BaseAddress = new Uri(provider.GetRequiredService<NavigationManager>().BaseUri, UriKind.Absolute);
            return new HttpTemplateLibrary(client);
        });
        services.AddScoped<TemplateEffects>();
        services.AddScoped<TemplateSessionStore>();
        services.AddScoped<ITemplateStore>(provider => provider.GetRequiredService<TemplateSessionStore>());
    }

    public void Use(object host)
    {
        // The page is a Razor component of this assembly, so the host needs the
        // assembly among its route assemblies before it can resolve the route.
        if (host is IServiceProvider provider)
        {
            provider.GetRequiredService<IRouteAssemblyRegistry>().Add(typeof(TinyToolsFrontendPlugin).Assembly);
        }
    }
}
