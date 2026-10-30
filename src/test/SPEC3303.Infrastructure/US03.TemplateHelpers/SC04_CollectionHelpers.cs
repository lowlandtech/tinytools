namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US03.TemplateHelpers;

[Trait(Spec.SPEC, "3303")]
[Trait(Spec.SC, "04")]
[UserStory("03", "Templates transform collections through deterministic helpers")]
public sealed class SC04_CollectionHelpers : IClassFixture<UseCases.CollectionHelpersUseCase>
{
    private readonly UseCases.CollectionHelpersUseCase useCase;
    private TinyTemplateEngine engine = null!;
    private ExecutionContext context = null!;
    private Dictionary<string, object?> ada = null!;
    private Dictionary<string, object?> ben = null!;
    private Dictionary<string, object?> cy = null!;

    public SC04_CollectionHelpers(UseCases.CollectionHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("typed collection values with properties, null input and comparable keys")]
    private void Given()
    {
        engine = new TinyTemplateEngine();
        context = new ExecutionContext();
        ada = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["Name"] = "Ada", ["Active"] = true, ["Team"] = "A", ["Rank"] = 2 };
        ben = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["Name"] = "Ben", ["Active"] = false, ["Team"] = "B", ["Rank"] = 1 };
        cy = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["Name"] = "Cy", ["Active"] = true, ["Team"] = "A", ["Rank"] = 2 };
        context.Set("Items", new object?[] { ada, ben, cy, ada });
        context.Set("NullItems", null);
        context.Set("EmptyItems", Array.Empty<object?>());
    }

    [When("the public expression resolver applies each collection helper")]
    private void When()
    {
        useCase.Cases["FilteredSequence"] = engine.ResolveExpression("Context.Items | where:Active==true", context);
        useCase.Cases["EmptyFilteredSequence"] = engine.ResolveExpression("Context.EmptyItems | where:Active==true", context);
        useCase.Cases["NullFilteredSequence"] = engine.ResolveExpression("Context.NullItems | where:Active==true", context);
        useCase.Cases["OrderedSequence"] = engine.ResolveExpression("Context.Items | orderby:Rank", context);
        try { useCase.Cases["OrderFailureResult"] = engine.ResolveExpression("Context.Items | orderby:Missing", context); }
        catch (ArgumentException error) { useCase.Cases["OrderFailureResult"] = error.GetType().Name + ":" + error.Message; }
        useCase.Cases["GroupedSequence"] = ((IEnumerable<IGrouping<object?, object?>>)engine.ResolveExpression("Context.Items | groupby:Team", context)!).Select(group => $"{group.Key}-first-{string.Join("-", group.Select(item => ((Dictionary<string, object?>)item!)["Name"]))}").ToArray();
        useCase.Cases["DistinctSequence"] = engine.ResolveExpression("Context.Items | distinct", context);
        useCase.Cases["GroupedNullResult"] = engine.ResolveExpression("Context.NullItems | groupby:Team", context);
        try { useCase.Cases["InvalidHelperResult"] = engine.ResolveExpression("Context.Items | where:Active===true", context); }
        catch (ArgumentException error) { useCase.Cases["InvalidHelperResult"] = error.GetType().Name + ":" + error.Message; }
        useCase.Cases["ChainedSequence"] = engine.ResolveExpression("Context.Items | where:Active==true | distinct", context);
    }

    [Then("Result.FilteredSequence should have count 3")]
    [Trait("UAC", "01"), Fact(Skip = "TODO: implement the declared where predicate grammar")]
    public void UAC01_FiltersInSourceOrder() => ((IEnumerable<object?>)useCase.Cases["FilteredSequence"]!).Should().BeEquivalentTo(new object?[] { ada, cy, ada }, options => options.WithStrictOrdering());

    [Then("Result.EmptyFilteredSequence should have count 0")]
    [Trait("UAC", "02"), Fact(Skip = "TODO: implement empty collection filtering")]
    public void UAC02_EmptyInput() => ((IEnumerable<object?>)useCase.Cases["EmptyFilteredSequence"]!).Should().BeEmpty();

    [Then("Result.NullFilteredSequence should have count 0")]
    [Trait("UAC", "03"), Fact(Skip = "TODO: implement null collection policy")]
    public void UAC03_NullInput() => ((IEnumerable<object?>)useCase.Cases["NullFilteredSequence"]!).Should().BeEmpty();

    [Then("Result.OrderedSequence should have count 4")]
    [Trait("UAC", "04"), Fact(Skip = "TODO: implement stable invariant ordering")]
    public void UAC04_OrdersStably() => ((IEnumerable<object?>)useCase.Cases["OrderedSequence"]!).Should().BeEquivalentTo(new object?[] { ben, ada, cy, ada }, options => options.WithStrictOrdering());

    [Then("OrderBy should fail with ArgumentException")]
    [Trait("UAC", "05"), Fact(Skip = "TODO: implement named invalid order paths")]
    public void UAC05_MissingOrderPath() => ((string)useCase.Cases["OrderFailureResult"]!).Should().StartWith("ArgumentException:");

    [Then("Result.GroupedSequence should be A-first-Ada-Cy-Ada-B-first-Ben")]
    [Trait("UAC", "06"), Fact(Skip = "TODO: implement stable grouping")]
    public void UAC06_GroupsInFirstSeenOrder() => ((string[])useCase.Cases["GroupedSequence"]!).Should().Equal("A-first-Ada-Cy-Ada", "B-first-Ben");

    [Then("Result.DistinctSequence should have count 3")]
    [Trait("UAC", "07"), Fact(Skip = "TODO: implement equality-based distinct")]
    public void UAC07_KeepsFirstEqualValue() => ((IEnumerable<object?>)useCase.Cases["DistinctSequence"]!).Should().BeEquivalentTo(new object?[] { ada, ben, cy }, options => options.WithStrictOrdering());

    [Then("Result.GroupedNullSequence should have count 0")]
    [Trait("UAC", "08"), Fact(Skip = "TODO: implement null collection policy for groupby")]
    public void UAC08_GroupsNullAsEmpty() => ((IEnumerable<object?>)useCase.Cases["GroupedNullResult"]!).Should().BeEmpty();

    [Then("Result.ChainedSequence should have count 2")]
    [Trait("UAC", "09"), Fact(Skip = "TODO: implement left-to-right collection helper composition")]
    public void UAC09_ComposesHelpersLeftToRight() => ((IEnumerable<object?>)useCase.Cases["ChainedSequence"]!).Should().BeEquivalentTo(new object?[] { ada, cy }, options => options.WithStrictOrdering());
}
