using LowlandTech.TinyTools.Api.Domain;
using LowlandTech.TinyTools.Api.Endpoints;
using LowlandTech.TinyTools.Api.Services;
using LowlandTech.TinyTools.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedPlugin = LowlandTech.Plugins.IPlugin;

namespace LowlandTech.TinyTools.Api.Plugins;

/// <summary>Registers, installs and activates the API package in the host lifecycle.</summary>
public sealed class TinyToolsMountedPlugin : SharedPlugin
{
    public LowlandTech.Plugins.PluginLifecycleDescriptor Descriptor { get; } = new()
    {
        PluginId = TinyToolsPluginIdentity.PluginId,
        Version = TinyToolsPluginIdentity.Version,
        Publisher = TinyToolsPluginIdentity.Publisher,
        LifecycleVersion = LowlandTech.Plugins.PluginLifecycleContract.Version,
        Targets = [LowlandTech.Plugins.PluginTarget.Backend],
        Phases = ["register", "install", "activate", "withdraw"],
        RequestedConnection = null,
        RequestsPluginOwnedReceiptStore = false,
    };

    public ValueTask<LowlandTech.Plugins.PluginPhaseResult> RegisterAsync(
        LowlandTech.Plugins.PluginRegistrationContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Services.AddSingleton<ITemplateExecutor>(new TemplateExecutor());
        context.Services.AddSingleton<IdempotencyStore>(new IdempotencyStore());
        context.Services.AddSingleton<IDbContextFactory<TinyToolsDbContext>>(services =>
            new PooledDbContextFactory<TinyToolsDbContext>(
                TinyToolsDbContextOptions.Create(services.GetRequiredService<IConfiguration>())));
        context.Services.AddScoped<TinyToolsDbContext>(services =>
            services.GetRequiredService<IDbContextFactory<TinyToolsDbContext>>().CreateDbContext());
        context.Services.AddScoped<ITemplateLibrary>(services => new TemplateService(
            services.GetRequiredService<TinyToolsDbContext>(),
            services.GetRequiredService<ITemplateExecutor>()));

        return new(LowlandTech.Plugins.PluginPhaseResult.Completed(
            LowlandTech.Plugins.PluginLifecyclePhase.Register,
            "registered Tinytools services against host configuration"));
    }

    public async ValueTask<LowlandTech.Plugins.PluginPhaseResult> InstallAsync(
        LowlandTech.Plugins.PluginInstallContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.Connection is null)
        {
            return LowlandTech.Plugins.PluginPhaseResult.Refused(
                LowlandTech.Plugins.PluginLifecyclePhase.Install,
                LowlandTech.Plugins.PluginLifecycleRefusalCode.MissingHostConnection,
                "The host published no connection to install Tinytools against.");
        }

        if (context.IsReplay)
        {
            return LowlandTech.Plugins.PluginPhaseResult.Migrated(
                LowlandTech.Plugins.PluginLifecyclePhase.Install,
                LowlandTech.Plugins.PluginMigrationOutcome.AlreadyApplied,
                "replayed Tinytools installation against the host connection");
        }

        if (!context.Services.TryGetValue(typeof(IDbContextFactory<TinyToolsDbContext>), out var registered)
            || registered is not IDbContextFactory<TinyToolsDbContext> factory)
        {
            return LowlandTech.Plugins.PluginPhaseResult.Refused(
                LowlandTech.Plugins.PluginLifecyclePhase.Install,
                LowlandTech.Plugins.PluginLifecycleRefusalCode.MigrationFailed,
                "Tinytools could not resolve the database factory it registered.");
        }

        try
        {
            await using var database = await factory.CreateDbContextAsync(cancellationToken);
            await database.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return LowlandTech.Plugins.PluginPhaseResult.Refused(
                LowlandTech.Plugins.PluginLifecyclePhase.Install,
                LowlandTech.Plugins.PluginLifecycleRefusalCode.MigrationFailed,
                "The Tinytools schema migration against the host connection failed.",
                error.Message);
        }

        return LowlandTech.Plugins.PluginPhaseResult.Migrated(
            LowlandTech.Plugins.PluginLifecyclePhase.Install,
            LowlandTech.Plugins.PluginMigrationOutcome.Applied,
            "migrated the Tinytools schema against the host connection");
    }

    public ValueTask<LowlandTech.Plugins.PluginPhaseResult> ActivateAsync(
        LowlandTech.Plugins.PluginActivationContext context,
        CancellationToken cancellationToken) =>
        new(LowlandTech.Plugins.PluginPhaseResult.Completed(
            LowlandTech.Plugins.PluginLifecyclePhase.Activate,
            "activated the Tinytools endpoint contribution"));

    public ValueTask<LowlandTech.Plugins.PluginPhaseResult> WithdrawAsync(
        LowlandTech.Plugins.PluginWithdrawContext context,
        CancellationToken cancellationToken) =>
        new(LowlandTech.Plugins.PluginPhaseResult.Completed(
            LowlandTech.Plugins.PluginLifecyclePhase.Withdraw,
            "withdrew Tinytools routes while retaining template data"));
}
