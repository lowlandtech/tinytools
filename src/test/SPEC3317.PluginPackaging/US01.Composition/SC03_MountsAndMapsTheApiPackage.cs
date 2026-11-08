using LowlandTech.Core.Plugins;
using LowlandTech.Plugins;
using LowlandTech.TinyTools.Api.Plugins;

namespace LowlandTech.TinyTools.Tests.SPEC3317.PluginPackaging.US01.Composition;

[Trait(Spec.SPEC, "3317")]
[Trait(Spec.SC, "03")]
[UserStory("01", "Tinytools ships as composable Harness plugin packages")]
public sealed class SC03_MountsAndMapsTheApiPackage : IClassFixture<UseCases.PluginPackagingUseCase>
{
    private readonly UseCases.PluginPackagingUseCase useCase;

    public SC03_MountsAndMapsTheApiPackage(UseCases.PluginPackagingUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("the Tinytools API package declares both host plugin contracts")]
    private void Given()
    {
        useCase.Cases["MountedPluginType"] = typeof(TinyToolsMountedPlugin);
        useCase.Cases["ApiPluginType"] = typeof(TinyToolsApiPlugin);
    }

    [When("the Core feature package registers and maps its portable endpoint group")]
    private void When()
    {
        var api = new TinyToolsApiPlugin();
        var registry = new EndpointGroupRegistry();
        var features = new FeatureRegistry();
        foreach (var feature in api.Features)
            feature.Register(new FeatureRegistrationBuilder(features, api.Descriptor.Id, endpointRegistry: registry));

        var declaration = registry.All.Single();
        var group = (EndpointGroup)Activator.CreateInstance(declaration.GroupType)!;
        var context = new EndpointGroupContextSpy();
        group.MapEndpoints(context);

        useCase.Cases["MountedContract"] = typeof(IPlugin).IsAssignableFrom((Type)useCase.Cases["MountedPluginType"]!);
        useCase.Cases["ApiContract"] = typeof(IApiPlugin).IsAssignableFrom((Type)useCase.Cases["ApiPluginType"]!);
        useCase.Cases["Identity"] = api.Descriptor.Id;
        useCase.Cases["Version"] = api.Descriptor.Version == TinyToolsPluginIdentity.Version;
        useCase.Cases["Prefix"] = declaration.GroupPrefix;
        useCase.Cases["Routes"] = string.Join("|", context.Mappings);
    }

    [Then("the mounted plugin contract should be true")]
    [Trait("UAC", "17"), Fact]
    public void UAC17_DeclaresMountedPlugin() => ((bool)useCase.Cases["MountedContract"]!).Should().BeTrue();

    [Then("the Core API plugin contract should be true")]
    [Trait("UAC", "18"), Fact]
    public void UAC18_DeclaresCoreApiPlugin() => ((bool)useCase.Cases["ApiContract"]!).Should().BeTrue();

    [Then("the API plugin identity should be tinytools")]
    [Trait("UAC", "19"), Fact]
    public void UAC19_UsesTheManifestIdentity() => ((string)useCase.Cases["Identity"]!).Should().Be("tinytools");

    [Then("the package version check should be true")]
    [Trait("UAC", "20"), Fact]
    public void UAC20_UsesThePackageVersion() => ((bool)useCase.Cases["Version"]!).Should().BeTrue();

    [Then("the endpoint prefix should be /api/templates")]
    [Trait("UAC", "21"), Fact]
    public void UAC21_DeclaresResourcePath() => ((string)useCase.Cases["Prefix"]!).Should().Be("/api/templates");

    [Then("the template route list should be exact")]
    [Trait("UAC", "22"), Fact]
    public void UAC22_MapsTheTemplateResources() => ((string)useCase.Cases["Routes"]!).Should().Be("GET /|GET /{id:guid}|POST /|PUT /{id:guid}|DELETE /{id:guid}|POST /render|POST /versions/{id:guid}/validate");
}
