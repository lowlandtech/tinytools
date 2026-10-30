namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US01.ExecutionContext;

[Trait("SPEC", "3303"), Trait("US", "01"), Trait("SC", "02")]
[UserStory("01", "Tool context manages variables and state")]
public sealed class SC02_PushesAndPopsCursor : IClassFixture<UseCases.ExecutionContextUseCase>
{
    private readonly UseCases.ExecutionContextUseCase useCase;
    private global::LowlandTech.TinyTools.ExecutionContext context = null!;
    private object item = null!;

    public SC02_PushesAndPopsCursor(UseCases.ExecutionContextUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a context and one cursor item carrying an index and a key")]
    private void Given()
    {
        context = new global::LowlandTech.TinyTools.ExecutionContext();
        item = new { Name = "Test" };
    }

    [When("the cursor is pushed once and popped once")]
    private void When()
    {
        context.PushCursor(item, 5, "key1");
        useCase.Cases["item"] = item;
        useCase.Cases["PushedCurrent"] = context.Current;
        useCase.Cases["PushedIndex"] = context.CurrentIndex;
        useCase.Cases["PushedKey"] = context.CurrentKey;
        useCase.Cases["PushedDepth"] = context.CursorDepth;
        context.PopCursor();
        useCase.Cases["PoppedCurrent"] = context.Current;
        useCase.Cases["PoppedIndex"] = context.CurrentIndex;
        useCase.Cases["PoppedKey"] = context.CurrentKey;
        useCase.Cases["PoppedDepth"] = context.CursorDepth;
    }

    [Then("the pushed current should be item")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_PushesTheItem() => useCase.Cases["PushedCurrent"].Should().BeSameAs(useCase.Cases["item"]);

    [Then("the pushed index should be 5")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_PushesTheIndex() => useCase.Cases["PushedIndex"].Should().Be(5);

    [Then("the pushed key should be key1")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_PushesTheKey() => useCase.Cases["PushedKey"].Should().Be("key1");

    [Then("the pushed depth should be 1")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_CountsThePushedDepth() => useCase.Cases["PushedDepth"].Should().Be(1);

    [Then("the popped current should be null")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_PopsTheCurrent() => useCase.Cases["PoppedCurrent"].Should().BeNull();

    [Then("the popped index should be -1")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_PopsTheIndex() => useCase.Cases["PoppedIndex"].Should().Be(-1);

    [Then("the popped key should be null")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_PopsTheKey() => useCase.Cases["PoppedKey"].Should().BeNull();

    [Then("the popped depth should be 0")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_PopsTheDepth() => useCase.Cases["PoppedDepth"].Should().Be(0);
}
