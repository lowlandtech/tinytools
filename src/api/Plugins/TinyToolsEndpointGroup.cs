using LowlandTech.Core.Plugins;
using LowlandTech.TinyTools.Api.Endpoints;

namespace LowlandTech.TinyTools.Api.Plugins;

/// <summary>The resource endpoint group of the Tinytools backend package.</summary>
public sealed class TinyToolsEndpointGroup : EndpointGroup
{
    public override string GroupPrefix => "/api/templates";

    public override string[] Tags => ["Templates"];

    public override void MapEndpoints(IEndpointGroupContext context) => context.MapTemplateEndpoints();
}
