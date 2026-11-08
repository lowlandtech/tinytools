using System.Text.Json;
using System.Text.Json.Serialization;
using LowlandTech.Plugins;

namespace LowlandTech.TinyTools.Tests.SPEC3317.PluginPackaging.US01.Composition.UseCases;

/// <summary>Caller goal and captured result vocabulary only; executable API usage stays in each scenario.</summary>
public sealed class PluginPackagingUseCase : IDisposable
{
    private bool executed;

    public Dictionary<string, object?> Cases { get; } = new(StringComparer.Ordinal);

    public void RunOnce(Action given, Action when)
    {
        lock (Cases)
        {
            if (executed) return;
            given();
            when();
            executed = true;
        }
    }

    public void Dispose()
    {
        foreach (var value in Cases.Values.OfType<IDisposable>())
            value.Dispose();
    }
}
