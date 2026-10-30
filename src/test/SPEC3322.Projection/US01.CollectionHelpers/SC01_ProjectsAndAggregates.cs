namespace LowlandTech.TinyTools.Tests.SPEC3322.Projection.US01.CollectionHelpers;

[Trait(Spec.SPEC, "3322")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Templates project and aggregate collection values")]
public sealed class SC01_ProjectsAndAggregates : IClassFixture<UseCases.ProjectionUseCase>
{
    private readonly UseCases.ProjectionUseCase useCase;
    private readonly TinyTemplateEngine engine = new();
    private ExecutionContext context = null!;

    public SC01_ProjectsAndAggregates(UseCases.ProjectionUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a collection of items with names, scores, active flags and a mixed numeric list")]
    private void Given()
    {
        context = new ExecutionContext();
        context.Set("Items", new[]
        {
            new UseCases.ProjectionUseCase.Item { Name = "Ada", Team = "A", Active = true, Score = 3.5m },
            new UseCases.ProjectionUseCase.Item { Name = "Ben", Team = "B", Active = false, Score = 1.5m },
            new UseCases.ProjectionUseCase.Item { Name = "Cy", Team = "A", Active = true, Score = 5m },
        });
        context.Set("Names", new[] { "Ada", "Ben", "Ada" });
        context.Set("Mixed", new object?[] { 1, 2.5m, "skip", null, 3 });
        context.Set("Empty", Array.Empty<object?>());
    }

    [When("each projection and aggregate helper runs through the public expression resolver")]
    private void When()
    {
        useCase.Cases["Selected"] = Join(engine.ResolveExpression("Context.Items | select:Name", context));
        useCase.Cases["Scores"] = engine.ResolveExpression("Context.Mixed | sum", context);
        useCase.Cases["Average"] = engine.ResolveExpression("Context.Mixed | avg", context);
        useCase.Cases["Minimum"] = engine.ResolveExpression("Context.Mixed | min", context);
        useCase.Cases["Maximum"] = engine.ResolveExpression("Context.Mixed | max", context);
        useCase.Cases["EmptySum"] = engine.ResolveExpression("Context.Empty | sum", context);
        useCase.Cases["EmptyAverage"] = engine.ResolveExpression("Context.Empty | avg", context);
        useCase.Cases["Missing"] = Capture(() => engine.ResolveExpression("Context.Items | select:Nope", context));
        useCase.Cases["Chained"] = engine.ResolveExpression("Context.Items | where:Active==true | select:Name | join:-, ", context);
        useCase.Cases["Duplicates"] = Join(engine.ResolveExpression("Context.Names | select:Length", context));
    }

    private static string Join(object? value) => value is IEnumerable<object?> items ? string.Join(",", items.Select(item => item?.ToString() ?? "null")) : "none";

    private static string Capture(Func<object?> resolve)
    {
        try { resolve(); return "no-throw"; }
        catch (ArgumentException error) { return error.Message; }
    }

    [Then("the selected names should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ProjectsOneMember() => ((string)useCase.Cases["Selected"]!).Should().Be("Ada,Ben,Cy");

    [Then("the sum should be 6.5")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_SumsNumbers() => Convert.ToDecimal(useCase.Cases["Scores"]).Should().Be(6.5m);

    [Then("the average should be 2.1666666666666666666666666667")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_AveragesNumbers() => Convert.ToDecimal(useCase.Cases["Average"]).Should().Be(6.5m / 3);

    [Then("the minimum should be 1")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_FindsTheMinimum() => Convert.ToDecimal(useCase.Cases["Minimum"]).Should().Be(1m);

    [Then("the maximum should be 3")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_FindsTheMaximum() => Convert.ToDecimal(useCase.Cases["Maximum"]).Should().Be(3m);

    [Then("the empty sum should be 0")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_SumsAnEmptySequence() => Convert.ToDecimal(useCase.Cases["EmptySum"]).Should().Be(0m);

    [Then("the empty average should be null")]
    [Trait("UAC", "07"), Fact]
    public void UAC06b_AveragesNothingAsNull() => useCase.Cases["EmptyAverage"].Should().BeNull();

    [Then("the missing path should be refused")]
    [Trait("UAC", "08"), Fact]
    public void UAC04b_RefusesAMissingMember() => ((string)useCase.Cases["Missing"]!).Should().Contain("'Nope' is not a member");

    [Then("the chained text should be exact")]
    [Trait("UAC", "09"), Fact]
    public void UAC05b_ChainsLeftToRight() => ((string)useCase.Cases["Chained"]!).Should().Be("Ada-, Cy");

    [Then("the duplicate lengths should be exact")]
    [Trait("UAC", "10"), Fact]
    public void UAC01b_KeepsDuplicatesAndOrder() => ((string)useCase.Cases["Duplicates"]!).Should().Be("3,3,3");
}
