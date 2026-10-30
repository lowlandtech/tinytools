using RazorLight;
using RazorLight.Caching;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US11.RazorLightCache;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "03")]
[UserStory("11", "The RazorLight caching contract behaves predictably")]
public sealed class SC03_IsolatesConcurrentAndInvalidKeys : IClassFixture<UseCases.RazorLightCacheUseCase>
{
    private readonly UseCases.RazorLightCacheUseCase useCase;

    public SC03_IsolatesConcurrentAndInvalidKeys(UseCases.RazorLightCacheUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a provider shared by concurrent callers and an invalid key")]
    private void Given()
    {
    }

    [When("sixty-four callers cache and read their own key once, then an invalid key is added")]
    private void When()
    {
        var cache = new MemoryCachingProvider();
        var failures = 0;
        var sources = new System.Collections.Concurrent.ConcurrentBag<string>();

        Parallel.For(0, 64, index =>
        {
            var key = "k" + index;
            try
            {
                cache.CacheTemplate(key, () => new TemplatePage("s" + key, key));
                sources.Add(cache.RetrieveTemplate(key).Template.TemplatePageFactory().Source);
            }
            catch (Exception)
            {
                Interlocked.Increment(ref failures);
            }
        });

        useCase.Cases["Failures"] = failures;
        useCase.Cases["Distinct"] = sources.Distinct().Count();

        try
        {
            cache.CacheTemplate(" ", () => new TemplatePage("x", "x"));
            useCase.Cases["InvalidKey"] = "no-throw";
        }
        catch (ArgumentException error)
        {
            useCase.Cases["InvalidKey"] = error.GetType().Name;
        }
    }

    [Then("the concurrent failures should be 0")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_SurvivesConcurrentUse() => ((int)useCase.Cases["Failures"]!).Should().Be(0);

    [Then("the concurrent distinct should be 64")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_KeepsPerKeyIsolation() => ((int)useCase.Cases["Distinct"]!).Should().Be(64);

    [Then("the invalid key should be ArgumentException")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_RejectsAnInvalidKey() => ((string)useCase.Cases["InvalidKey"]!).Should().Be("ArgumentException");
}
