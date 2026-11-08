using LowlandTech.Core.Plugins;

namespace LowlandTech.TinyTools.Tests.SPEC3317.PluginPackaging.US01.Composition;

/// <summary>Records the portable route declarations a plugin endpoint group contributes.</summary>
public sealed class EndpointGroupContextSpy : IEndpointGroupContext, IServiceProvider
{
    public IServiceProvider Services => this;

    public List<string> Mappings { get; } = [];

    public void Map(string method, string pattern, Delegate handler) => Mappings.Add($"{method} {pattern}");

    public object? GetService(Type serviceType) => null;
}
