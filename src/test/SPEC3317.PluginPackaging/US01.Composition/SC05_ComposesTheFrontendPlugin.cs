using LowlandTech;
using LowlandTech.Core.Plugins;
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Frontend;
using LowlandTech.TinyTools.Frontend.Features.Templates;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LowlandTech.TinyTools.Tests.SPEC3317.PluginPackaging.US01.Composition;

[Trait(Spec.SPEC, "3317")]
[Trait(Spec.SC, "05")]
[UserStory("01", "Tinytools ships as composable Harness plugin packages")]
public sealed class SC05_ComposesTheFrontendPlugin : IClassFixture<UseCases.PluginPackagingUseCase>
{
    private readonly UseCases.PluginPackagingUseCase useCase;

    public SC05_ComposesTheFrontendPlugin(UseCases.PluginPackagingUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("the Tinytools frontend package and a browser host origin")]
    private void Given()
    {
        useCase.Cases["Plugin"] = new TinyToolsFrontendPlugin();
        useCase.Cases["ApiOrigin"] = "https://harness.example/";
    }

    [When("the host composes the plugin services and registers its route assembly")]
    private void When()
    {
        var plugin = (TinyToolsFrontendPlugin)useCase.Cases["Plugin"]!;
        var services = new ServiceCollection();
        services.AddSingleton<IRouteAssemblyRegistry, RouteAssemblyRegistry>();
        services.AddSingleton<NavigationManager>(new TestNavigationManager());
        services.AddScoped(_ => new HttpClient());
        plugin.ConfigureServices(services, new ConfigurationBuilder().Build());

        using var provider = services.BuildServiceProvider();
        plugin.Use(provider);
        using var scope = provider.CreateScope();
        var scoped = scope.ServiceProvider;
        useCase.Cases["Identity"] = plugin.Descriptor.Id;
        useCase.Cases["RouteAssembly"] = scoped.GetRequiredService<IRouteAssemblyRegistry>().Assemblies.Contains(typeof(TinyToolsFrontendPlugin).Assembly);
        _ = scoped.GetRequiredService<ITemplateLibrary>();
        useCase.Cases["ApiBase"] = scoped.GetRequiredService<HttpClient>().BaseAddress?.ToString();
        useCase.Cases["Library"] = scoped.GetRequiredService<ITemplateLibrary>() is not null;
        useCase.Cases["Effects"] = scoped.GetRequiredService<TemplateEffects>() is not null;
        useCase.Cases["SharedStore"] = ReferenceEquals(scoped.GetRequiredService<TemplateSessionStore>(), scoped.GetRequiredService<ITemplateStore>());
    }

    [Then("the plugin identity should be tinytools")]
    [Trait("UAC", "27"), Fact]
    public void UAC27_MatchesTheManifestKey() => ((string)useCase.Cases["Identity"]!).Should().Be("tinytools");

    [Then("the route assembly registration should be true")]
    [Trait("UAC", "28"), Fact]
    public void UAC28_RegistersItsRazorAssembly() => ((bool)useCase.Cases["RouteAssembly"]!).Should().BeTrue();

    [Then("the API base address should be https://harness.example/")]
    [Trait("UAC", "29"), Fact]
    public void UAC29_UsesTheHostOrigin() => ((string)useCase.Cases["ApiBase"]!).Should().Be("https://harness.example/");

    [Then("the template library service should be true")]
    [Trait("UAC", "30"), Fact]
    public void UAC30_ResolvesTemplateLibrary() => ((bool)useCase.Cases["Library"]!).Should().BeTrue();

    [Then("the template effects service should be true")]
    [Trait("UAC", "31"), Fact]
    public void UAC31_ResolvesTemplateEffects() => ((bool)useCase.Cases["Effects"]!).Should().BeTrue();

    [Then("the store identity check should be true")]
    [Trait("UAC", "32"), Fact]
    public void UAC32_UsesOneStoreInstance() => ((bool)useCase.Cases["SharedStore"]!).Should().BeTrue();

    [Then("TinyToolsApiClient.BaseAddress should be https://harness.example/")]
    [Trait("UAC", "34"), Fact]
    public void UAC34_ConfiguresTheHostHttpClient() => ((string)useCase.Cases["ApiBase"]!).Should().Be("https://harness.example/");
}
