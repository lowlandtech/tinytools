using Microsoft.AspNetCore.Components;

namespace LowlandTech.TinyTools.Tests.SPEC3317.PluginPackaging.US01.Composition;

/// <summary>Supplies the same-origin browser base URI to the frontend plugin scenario.</summary>
public sealed class TestNavigationManager : NavigationManager
{
    public TestNavigationManager() : this("https://harness.example/") { }

    public TestNavigationManager(string baseUri) => Initialize(baseUri, baseUri);

    protected override void NavigateToCore(string uri, bool forceLoad) => throw new NotSupportedException();

    protected override void NavigateToCore(string uri, NavigationOptions options) => throw new NotSupportedException();
}
