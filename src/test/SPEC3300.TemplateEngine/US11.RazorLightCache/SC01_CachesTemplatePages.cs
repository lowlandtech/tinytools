using RazorLight;
using RazorLight.Caching;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US11.RazorLightCache;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("11", "The RazorLight caching contract behaves predictably")]
public sealed class SC01_CachesTemplatePages : IClassFixture<UseCases.RazorLightCacheUseCase>
{
    private readonly UseCases.RazorLightCacheUseCase useCase;

    public SC01_CachesTemplatePages(UseCases.RazorLightCacheUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a memory provider with a cached page, a case-variant key and a replacement")]
    private void Given()
    {
    }

    [When("lookup, casing, replacement and removal run once")]
    private void When()
    {
        var cache = new MemoryCachingProvider();

        useCase.Cases["MissSuccess"] = cache.RetrieveTemplate("missing").Success;
        cache.CacheTemplate("greeting", () => new TemplatePage("Hello", "greeting"));
        var hit = cache.RetrieveTemplate("greeting");
        useCase.Cases["HitSuccess"] = hit.Success;
        useCase.Cases["HitKey"] = hit.Template.Key;
        useCase.Cases["FactorySource"] = hit.Template.TemplatePageFactory().Source;

        cache.CacheTemplate("Greeting", () => new TemplatePage("Cased", "Greeting"));
        useCase.Cases["OrdinalIsolation"] = cache.RetrieveTemplate("greeting").Template.TemplatePageFactory().Source;

        cache.CacheTemplate("greeting", () => new TemplatePage("Replaced", "greeting"));
        useCase.Cases["Replaced"] = cache.RetrieveTemplate("greeting").Template.TemplatePageFactory().Source;

        useCase.Cases["ContainsBefore"] = cache.Contains("greeting");
        cache.Remove("greeting");
        useCase.Cases["ContainsAfter"] = cache.Contains("greeting");
        cache.Remove("greeting");
    }

    [Then("the miss success should be false")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_MissIsExplicit() => ((bool)useCase.Cases["MissSuccess"]!).Should().BeFalse();

    [Then("the hit success should be true")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HitIsExplicit() => ((bool)useCase.Cases["HitSuccess"]!).Should().BeTrue();

    [Then("the hit key should be greeting")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_KeepsItsOwnKey() => ((string)useCase.Cases["HitKey"]!).Should().Be("greeting");

    [Then("the factory source should be Hello")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ProducesThePage() => ((string)useCase.Cases["FactorySource"]!).Should().Be("Hello");

    [Then("the cased source should be Hello")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_UsesTheOrdinalComparer() => ((string)useCase.Cases["OrdinalIsolation"]!).Should().Be("Hello");

    [Then("the replaced source should be Replaced")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ReplacesTheEntry() => ((string)useCase.Cases["Replaced"]!).Should().Be("Replaced");

    [Then("the contains before should be true")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ContainsTracksTheEntry() => ((bool)useCase.Cases["ContainsBefore"]!).Should().BeTrue();

    [Then("the contains after should be false")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_RemoveDropsTheEntry() => ((bool)useCase.Cases["ContainsAfter"]!).Should().BeFalse();
}
