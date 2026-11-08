using System.Text.Json;
using LowlandTech.TinyTools.Api.Plugins;
using LowlandTech.TinyTools.Frontend;
using LowlandTech.Plugins;

namespace LowlandTech.TinyTools.Tests.SPEC3317.PluginPackaging.US01.Composition;

[Trait(Spec.SPEC, "3317")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Tinytools ships as composable Harness plugin packages")]
public sealed class SC01_DeclaresPluginPackages : IClassFixture<UseCases.PluginPackagingUseCase>
{
    private readonly UseCases.PluginPackagingUseCase useCase;

    public SC01_DeclaresPluginPackages(UseCases.PluginPackagingUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    internal static PluginManifest Read(string package)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        var assembly = package switch
        {
            "api" => typeof(TinyToolsApiPlugin).Assembly,
            "frontend" => typeof(TinyToolsFrontendPlugin).Assembly,
            _ => throw new ArgumentOutOfRangeException(nameof(package), package, "Unknown package.")
        };
        using var stream = assembly.GetManifestResourceStream("lowlandtech/plugin.json")
            ?? throw new InvalidOperationException($"{package} package has no embedded plugin manifest.");
        return JsonSerializer.Deserialize<PluginManifest>(stream, options)!;
    }

    [Given("the frontend and api plugin manifests beside their project files")]
    private void Given()
    {
    }

    [When("both manifests are read through the Plugins.Domain model")]
    private void When()
    {
        var frontend = Read("frontend");
        var api = Read("api");
        useCase.Cases["FrontendTarget"] = frontend.Target;
        useCase.Cases["ApiTarget"] = api.Target;
        useCase.Cases["Schema"] = frontend.SchemaVersion;
        useCase.Cases["Route"] = frontend.Routes[0].Path;
        useCase.Cases["Page"] = frontend.Contributions.Pages[0];
        useCase.Cases["ApiRoutes"] = api.Routes.Count;
    }

    [Then("the frontend target should be Blazor")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_FrontendTargetsBlazor() => useCase.Cases["FrontendTarget"].Should().Be(PluginTarget.Blazor);

    [Then("the api target should be Backend")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ApiTargetsBackend() => useCase.Cases["ApiTarget"].Should().Be(PluginTarget.Backend);

    [Then("the schema version should be 3")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_DeclaresTheAssetSchemaVersion() => ((int)useCase.Cases["Schema"]!).Should().Be(3);

    [Then("the route path should be templates")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ContributesTheTemplatesRoute() => ((string)useCase.Cases["Route"]!).Should().Be("templates");

    [Then("the page contribution should be TemplatesPage")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ContributesTheTemplatesPage() => ((string)useCase.Cases["Page"]!).Should().Be("TemplatesPage");

    [Then("the api routes should have count 0")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ApiContributesNoRoutes() => ((int)useCase.Cases["ApiRoutes"]!).Should().Be(0);
}
