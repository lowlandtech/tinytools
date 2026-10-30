using RazorLight;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US11.RazorLightCache;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "02")]
[UserStory("11", "The RazorLight compatibility engine renders through the cache")]
public sealed class SC02_RendersThroughTheCache : IClassFixture<UseCases.RazorLightCacheUseCase>
{
    private readonly UseCases.RazorLightCacheUseCase useCase;

    public SC02_RendersThroughTheCache(UseCases.RazorLightCacheUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    private string _root = string.Empty;

    [Given("a file project with one template, a missing key and an escaping key")]
    private void Given()
    {
        _root = Path.Combine(Path.GetTempPath(), "tinytools-razorlight-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "greeting.cshtml"), "Hello, @Model.Name.");
    }

    [When("the engine compiles, caches and re-renders the template and the failure keys")]
    private void When()
    {
        var engine = new RazorLightEngineBuilder().UseFileSystemProject(_root).UseMemoryCachingProvider().Build();

        useCase.Cases["First"] = engine.CompileRenderAsync("greeting", new CacheViewModel("Ada")).GetAwaiter().GetResult();
        useCase.Cases["Cached"] = engine.Handler.Cache.RetrieveTemplate("greeting").Success;

        var page = engine.Handler.Cache.RetrieveTemplate("greeting").Template.TemplatePageFactory();
        useCase.Cases["Second"] = engine.RenderTemplateAsync(page, new CacheViewModel("Grace")).GetAwaiter().GetResult();

        var embedded = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(SC02_RendersThroughTheCache))
            .UseMemoryCachingProvider()
            .Build();
        useCase.Cases["Embedded"] = embedded.CompileRenderAsync("EmbeddedTemplate", new CacheViewModel("Embedded")).GetAwaiter().GetResult();

        try
        {
            engine.CompileTemplateAsync("absent").GetAwaiter().GetResult();
            useCase.Cases["Missing"] = "no-throw";
        }
        catch (Exception error)
        {
            useCase.Cases["Missing"] = error.GetType().Name;
        }

        try
        {
            engine.CompileTemplateAsync("../escape").GetAwaiter().GetResult();
            useCase.Cases["Escape"] = "no-throw";
        }
        catch (Exception error)
        {
            useCase.Cases["Escape"] = error.Message;
        }
    }

    [Then("the first render should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC06_RendersTheFirstModel() => ((string)useCase.Cases["First"]!).Should().Be("Hello, Ada.");

    [Then("the cached flag should be true")]
    [Trait("UAC", "02"), Fact]
    public void UAC06b_CachesTheCompiledPage() => ((bool)useCase.Cases["Cached"]!).Should().BeTrue();

    [Then("the second render should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC06c_RendersFreshModelData() => ((string)useCase.Cases["Second"]!).Should().Be("Hello, Grace.");

    [Then("the embedded render should be exact")]
    [Trait("UAC", "04"), Fact]
    public void UAC06d_RendersTheEmbeddedResource() => ((string)useCase.Cases["Embedded"]!).Should().Be("Hello, Embedded from embedded.");

    [Then("the missing type should be FileNotFoundException")]
    [Trait("UAC", "05"), Fact]
    public void UAC07_FailsOnAMissingTemplate() => ((string)useCase.Cases["Missing"]!).Should().Be("FileNotFoundException");

    [Then("the escape message should contain escapes the project root")]
    [Trait("UAC", "06"), Fact]
    public void UAC07b_RefusesRootEscape() => ((string)useCase.Cases["Escape"]!).Should().Contain("escapes the project root");
}
