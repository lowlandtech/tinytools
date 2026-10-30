namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US01.ExecutionContext;

[Trait("SPEC", "3303"), Trait("US", "01"), Trait("SC", "04")]
[UserStory("01", "Tool context manages variables and state")]
public sealed class SC04_UpdatesCursorVariables : IClassFixture<UseCases.ExecutionContextUseCase>
{
    private readonly UseCases.ExecutionContextUseCase useCase;
    private global::LowlandTech.TinyTools.ExecutionContext empty = null!;
    private global::LowlandTech.TinyTools.ExecutionContext pushed = null!;
    private global::LowlandTech.TinyTools.ExecutionContext idle = null!;
    private object item = null!;

    public SC04_UpdatesCursorVariables(UseCases.ExecutionContextUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("an empty context, a pushed context and a context without any cursor")]
    private void Given()
    {
        empty = new global::LowlandTech.TinyTools.ExecutionContext();
        pushed = new global::LowlandTech.TinyTools.ExecutionContext();
        idle = new global::LowlandTech.TinyTools.ExecutionContext();
        item = new { Value = 100 };
    }

    [When("an empty stack is popped and one cursor is pushed with an index and a key")]
    private void When()
    {
        empty.PopCursor();
        useCase.Cases["EmptyDepth"] = empty.CursorDepth;
        useCase.Cases["EmptyCurrent"] = empty.Current;
        pushed.PushCursor(item, 42, "mykey");
        useCase.Cases["item"] = item;
        useCase.Cases["VariableCurrent"] = pushed.Get("Current");
        useCase.Cases["VariableIndex"] = pushed.Get("CurrentIndex");
        useCase.Cases["VariableKey"] = pushed.Get("CurrentKey");
        useCase.Cases["NoCursorIndex"] = idle.CurrentIndex;
    }

    [Then("the empty depth should be 0")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_PopsAnEmptyStack() => useCase.Cases["EmptyDepth"].Should().Be(0);

    [Then("the empty current should be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_KeepsAnEmptyStackCurrent() => useCase.Cases["EmptyCurrent"].Should().BeNull();

    [Then("the variable current should be item")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_UpdatesTheCurrentVariable() => useCase.Cases["VariableCurrent"].Should().BeSameAs(useCase.Cases["item"]);

    [Then("the variable index should be 42")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_UpdatesTheIndexVariable() => useCase.Cases["VariableIndex"].Should().Be(42);

    [Then("the variable key should be mykey")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_UpdatesTheKeyVariable() => useCase.Cases["VariableKey"].Should().Be("mykey");

    [Then("the idle index should be -1")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ReportsNoCursorIndex() => useCase.Cases["NoCursorIndex"].Should().Be(-1);
}
