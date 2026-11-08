using System.Reflection;

namespace LowlandTech.TinyTools.Api.Plugins;

/// <summary>The one identity shared by the API package's mounted plugin and Core declaration.</summary>
public static class TinyToolsPluginIdentity
{
    public const string PluginId = "tinytools";
    public const string PluginName = "Tinytools";
    public const string Publisher = "LowlandTech";
    public const string VersionMetadataKey = "TinyToolsPluginVersion";

    public static string Version { get; } = ReadVersion();

    private static string ReadVersion() =>
        typeof(TinyToolsPluginIdentity).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .SingleOrDefault(attribute => attribute.Key == VersionMetadataKey)?.Value
        ?? throw new InvalidOperationException("The TinyTools API package version metadata is missing.");
}
