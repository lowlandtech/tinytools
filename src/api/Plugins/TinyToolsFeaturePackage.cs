using LowlandTech.Core.Plugins;

namespace LowlandTech.TinyTools.Api.Plugins;

/// <summary>Declares the template resources to the host's generic endpoint-group registry.</summary>
public sealed class TinyToolsFeaturePackage : IFeaturePackage
{
    public const string FeatureId = "tinytools.templates";

    public string PluginId => TinyToolsPluginIdentity.PluginId;

    public void Register(IFeatureRegistrationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.MapEndpoints<TinyToolsEndpointGroup>(FeatureId);
    }
}
