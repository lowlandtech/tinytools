namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US07.CompiledTemplates;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("07", "A compiled template plan is reused across changing contexts")]
public sealed class SC01_ReusesCompiledPlans : IClassFixture<UseCases.CompiledTemplatesUseCase>
{
    private readonly UseCases.CompiledTemplatesUseCase useCase;

    public SC01_ReusesCompiledPlans(UseCases.CompiledTemplatesUseCase useCase)
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

    [Given("separate engines for context isolation, plan reuse and a one-character source change")]
    private void Given()
    {
        var isolation = new TinyTemplateEngine();
        var plan = isolation.Compile("Hello ${Name}");
        useCase.Cases["First"] = plan.Render(ContextWith("Name", "Ada"));
        useCase.Cases["Second"] = plan.Render(ContextWith("Name", "Grace"));

        var reuse = new TinyTemplateEngine();
        useCase.Cases["PlanA"] = reuse.Compile("Hello ${Name}");
        useCase.Cases["PlanB"] = reuse.Compile("Hello ${Name}");
        useCase.Cases["ReuseOutput"] = ((CompiledTemplate)useCase.Cases["PlanA"]!).Render(ContextWith("Name", "Ada"));
        useCase.Cases["RenderedOutput"] = reuse.Render("Hello ${Name}", ContextWith("Name", "Ada"));

        var change = new TinyTemplateEngine();
        useCase.Cases["Before"] = change.Compile("Value: ${Name}");
        useCase.Cases["After"] = change.Compile("Value: ${Name}!").Render(ContextWith("Name", "Ada"));
    }

    [When("each case compiles and renders once")]
    private void When()
    {
    }

    [Then("the first rendering should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheFirstContext() => ((string)useCase.Cases["First"]!).Should().Be("Hello Ada");

    [Then("the second rendering should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheSecondContext() => ((string)useCase.Cases["Second"]!).Should().Be("Hello Grace");

    [Then("the reused plan should be identical")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ReusesTheCachedPlan() => useCase.Cases["PlanB"].Should().BeSameAs(useCase.Cases["PlanA"]);

    [Then("the compiled rendering should be equal")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_MatchesTheEngineRender() => useCase.Cases["ReuseOutput"].Should().Be(useCase.Cases["RenderedOutput"]);

    [Then("the changed plan should be distinct")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_CompilesAChangedSourceSeparately() => useCase.Cases["After"].Should().NotBe(useCase.Cases["Before"]);
}
