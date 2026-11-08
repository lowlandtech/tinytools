using LowlandTech.Plugins;

namespace LowlandTech.TinyTools.Tests.SPEC3317.PluginPackaging.US01.Composition;

/// <summary>A test receipt store; the mounted plugin must leave durable receipt ownership to the host.</summary>
public sealed class InMemoryInstallReceiptStore : IPluginInstallReceiptStore
{
    public string StoreId => "host-test-store";

    public Task<PluginInstallReceipt?> FindAsync(string pluginId, string pluginVersion, string hostDeployment, CancellationToken cancellationToken) =>
        Task.FromResult<PluginInstallReceipt?>(null);

    public Task RecordAsync(PluginInstallReceipt receipt, CancellationToken cancellationToken) => Task.CompletedTask;
}
