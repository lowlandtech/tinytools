namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US07.CompiledTemplates;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "02")]
[UserStory("07", "A compiled template plan is reused across changing contexts")]
public sealed class SC02_ReusesUnderConcurrencyAndEviction : IClassFixture<UseCases.CompiledTemplatesUseCase>
{
    private readonly UseCases.CompiledTemplatesUseCase useCase;

    public SC02_ReusesUnderConcurrencyAndEviction(UseCases.CompiledTemplatesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    private static ExecutionContext ContextWith(string name, string value)
    {
        var context = new ExecutionContext();
        context.Set(name, value);
        return context;
    }

    [Given("one engine under concurrent callers and a second engine filled to its 128-entry capacity")]
    private void Given()
    {
        var concurrent = new TinyTemplateEngine();
        var concurrentPlans = new System.Collections.Concurrent.ConcurrentBag<CompiledTemplate>();
        var outputs = new System.Collections.Concurrent.ConcurrentBag<string>();
        Parallel.For(0, 64, index =>
        {
            var compiled = concurrent.Compile("Hi ${Name}");
            concurrentPlans.Add(compiled);
            outputs.Add(compiled.Render(ContextWith("Name", $"caller{index}")));
        });
        useCase.Cases["ConcurrentPlans"] = concurrentPlans.Distinct().Count();
        useCase.Cases["ConcurrentOutputs"] = outputs.Distinct().Count();

        var cache = new TinyTemplateEngine();
        var sources = Enumerable.Range(0, 128).Select(index => $"Entry {index}: ${{Name}}").ToList();
        var compiledPlans = sources.Select(source => cache.Compile(source)).ToList();

        // Touch the oldest entry so the next entry becomes the least recently used one,
        // then overflow the cache by one to evict it.
        useCase.Cases["Touched"] = cache.Compile(sources[0]);
        cache.Compile("Entry 128: ${Name}");

        useCase.Cases["TouchedAfter"] = cache.Compile(sources[0]);
        useCase.Cases["ColdBefore"] = compiledPlans[1];
        useCase.Cases["ColdAfter"] = cache.Compile(sources[1]);
        useCase.Cases["EvictedOutput"] = ((CompiledTemplate)useCase.Cases["ColdAfter"]!).Render(ContextWith("Name", "Ada"));
    }

    [When("each case compiles and renders once")]
    private void When()
    {
    }

    [Then("the concurrent plan count should be 1")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_CompilesOnceUnderConcurrency() => ((int)useCase.Cases["ConcurrentPlans"]!).Should().Be(1);

    [Then("the concurrent output count should be 64")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_KeepsEachCallersOutput() => ((int)useCase.Cases["ConcurrentOutputs"]!).Should().Be(64);

    [Then("the touched plan should be identical")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_KeepsTheRecentlyUsedPlan() => useCase.Cases["TouchedAfter"].Should().BeSameAs(useCase.Cases["Touched"]);

    [Then("the cold plan should be distinct")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_RecompilesTheEvictedPlan() => useCase.Cases["ColdAfter"].Should().NotBeSameAs(useCase.Cases["ColdBefore"]);

    [Then("the evicted rendering should be exact")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_KeepsTheEvictedRenderCorrect() => ((string)useCase.Cases["EvictedOutput"]!).Should().Be("Entry 1: Ada");
}
