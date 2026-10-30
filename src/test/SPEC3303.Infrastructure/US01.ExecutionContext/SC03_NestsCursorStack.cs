namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US01.ExecutionContext;

[Trait("SPEC", "3303"), Trait("US", "01"), Trait("SC", "03")]
[UserStory("01", "Tool context manages variables and state")]
public sealed class SC03_NestsCursorStack : IClassFixture<UseCases.ExecutionContextUseCase>
{
    private readonly UseCases.ExecutionContextUseCase useCase;
    private global::LowlandTech.TinyTools.ExecutionContext context = null!;
    private object first = null!;
    private object second = null!;
    private object third = null!;

    public SC03_NestsCursorStack(UseCases.ExecutionContextUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a context and three cursor items pushed in order")]
    private void Given()
    {
        context = new global::LowlandTech.TinyTools.ExecutionContext();
        first = new { Name = "First" };
        second = new { Name = "Second" };
        third = new { Name = "Third" };
    }

    [When("three cursors are pushed and then popped one by one")]
    private void When()
    {
        context.PushCursor(first, 0);
        context.PushCursor(second, 1);
        context.PushCursor(third, 2);
        useCase.Cases["third"] = third;
        useCase.Cases["StackedCurrent"] = context.Current;
        useCase.Cases["StackedIndex"] = context.CurrentIndex;
        useCase.Cases["StackedDepth"] = context.CursorDepth;
        context.PopCursor();
        useCase.Cases["second"] = second;
        useCase.Cases["PoppedCurrent"] = context.Current;
        useCase.Cases["PoppedIndex"] = context.CurrentIndex;
        context.PopCursor();
        useCase.Cases["first"] = first;
        useCase.Cases["DeeperCurrent"] = context.Current;
        useCase.Cases["DeeperIndex"] = context.CurrentIndex;
        context.PopCursor();
        useCase.Cases["EmptiedCurrent"] = context.Current;
    }

    [Then("the stacked current should be third")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_KeepsTheLastPush() => useCase.Cases["StackedCurrent"].Should().BeSameAs(useCase.Cases["third"]);

    [Then("the stacked index should be 2")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_KeepsTheLastIndex() => useCase.Cases["StackedIndex"].Should().Be(2);

    [Then("the stacked depth should be 3")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_CountsThreeEntries() => useCase.Cases["StackedDepth"].Should().Be(3);

    [Then("the popped current should be second")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RevealsTheSecondItem() => useCase.Cases["PoppedCurrent"].Should().BeSameAs(useCase.Cases["second"]);

    [Then("the popped index should be 1")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RevealsTheSecondIndex() => useCase.Cases["PoppedIndex"].Should().Be(1);

    [Then("the deeper current should be first")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RevealsTheFirstItem() => useCase.Cases["DeeperCurrent"].Should().BeSameAs(useCase.Cases["first"]);

    [Then("the deeper index should be 0")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_RevealsTheFirstIndex() => useCase.Cases["DeeperIndex"].Should().Be(0);

    [Then("the emptied current should be null")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_EmptiesTheStack() => useCase.Cases["EmptiedCurrent"].Should().BeNull();
}
