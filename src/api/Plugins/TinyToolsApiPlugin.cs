using LowlandTech.Core.Plugins;
using LowlandTech.TinyTools.Api.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LowlandTech.TinyTools.Api.Plugins;

/// <summary>The Core API declaration that contributes the backend endpoint group.</summary>
public sealed class TinyToolsApiPlugin : IApiPlugin
{
    public PluginDescriptor Descriptor { get; } = new()
    {
        Id = TinyToolsPluginIdentity.PluginId,
        Name = TinyToolsPluginIdentity.PluginName,
        Version = TinyToolsPluginIdentity.Version,
        Author = TinyToolsPluginIdentity.Publisher,
        Description = "Template resources contributed to the composed host.",
        SchemaName = TinyToolsDbContext.Schema,
        DbContextType = typeof(TinyToolsDbContext),
    };

    public IReadOnlyList<IFeaturePackage> Features { get; } = [new TinyToolsFeaturePackage()];

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContextFactory<TinyToolsDbContext>(options => TinyToolsDbContextOptions.Configure(options, configuration));
        services.AddScoped<TinyToolsDbContext>(provider =>
            provider.GetRequiredService<IDbContextFactory<TinyToolsDbContext>>().CreateDbContext());
        services.AddSingleton<LowlandTech.TinyTools.Core.Abstractions.ITemplateExecutor,
            LowlandTech.TinyTools.Api.Services.TemplateExecutor>();
        services.AddScoped<LowlandTech.TinyTools.Core.Abstractions.ITemplateLibrary,
            LowlandTech.TinyTools.Api.Services.TemplateService>();
        services.AddSingleton<LowlandTech.TinyTools.Api.Endpoints.IdempotencyStore>();
    }

    public void Use(object host)
    {
        if (host is not WebApplication app)
            throw new NotSupportedException("The Tinytools API package supports an ASP.NET Core WebApplication host.");

        LowlandTech.TinyTools.Api.Endpoints.TemplateEndpoints.MapTemplateEndpoints(app);
    }

    public async Task InstallAsync(IPluginInstallContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        var factory = context.Services.GetRequiredService<IDbContextFactory<TinyToolsDbContext>>();
        await using var database = await factory.CreateDbContextAsync(cancellationToken);
        await database.Database.MigrateAsync(cancellationToken);
    }

    public Task UninstallAsync(IPluginInstallContext context, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

internal static class TinyToolsDbContextOptions
{
    public static DbContextOptions<TinyToolsDbContext> Create(IConfiguration configuration)
    {
        var options = new DbContextOptionsBuilder<TinyToolsDbContext>();
        Configure(options, configuration);
        return options.Options;
    }

    public static void Configure(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (connectionString is not null && connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            options.UseNpgsql(connectionString);
        else
            options.UseSqlite(connectionString ?? "Data Source=tinytools.db");
    }
}
