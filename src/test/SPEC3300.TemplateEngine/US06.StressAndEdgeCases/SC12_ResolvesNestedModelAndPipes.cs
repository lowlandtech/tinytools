namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "12")]
[UserStory("06", "Variable resolution handles nested paths, pipes and null coalescing")]
public sealed class SC12_ResolvesNestedModelAndPipes : IClassFixture<UseCases.StressAndEdgeCasesUseCase>
{
    private readonly UseCases.StressAndEdgeCasesUseCase useCase;

    public SC12_ResolvesNestedModelAndPipes(UseCases.StressAndEdgeCasesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    private const string Template =
        "Customer: ${Context.Model.Customer.Address.City | upper}; Owner: ${Context.Owner | trim | upper}; Alias: ${Context.Alias ?? \"anonymous\"}";

    private static ExecutionContext Build(string? alias)
    {
        var context = new ExecutionContext();
        context.Set("Model", new { Customer = new { Address = new { City = "Amsterdam" } } });
        context.Set("Owner", " Ada ");
        if (alias is not null) context.Set("Alias", alias);
        return context;
    }

    [Given("the same template resolved against an absent and a populated alias")]
    private void Given()
    {
        var engine = new TinyTemplateEngine();
        useCase.Cases["Absent"] = engine.ResolveVariables(Template, Build(null));
        useCase.Cases["Populated"] = engine.ResolveVariables(Template, Build("ops"));
    }

    [When("the template is resolved once per alias variant")]
    private void When()
    {
    }

    [Then("the nested render should contain Amsterdam")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ResolvesTheNestedModelPath() => ((string)useCase.Cases["Absent"]!).Should().Contain("Customer: AMSTERDAM");

    [Then("the pipe render should contain ADA")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_AppliesTheChainedPipes() => ((string)useCase.Cases["Absent"]!).Should().Contain("Owner: ADA");

    [Then("the absent alias should contain anonymous")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_FallsBackForTheAbsentAlias() => ((string)useCase.Cases["Absent"]!).Should().Contain("Alias: anonymous");

    [Then("the populated alias should contain ops")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_UsesThePopulatedAlias() => ((string)useCase.Cases["Populated"]!).Should().Contain("Alias: ops");
}
