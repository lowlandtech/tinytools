namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US01.ExecutionContext;

[Trait("SPEC", "3303"), Trait("US", "01"), Trait("SC", "09")]
[UserStory("01", "Tool context manages variables and state")]
public sealed class SC09_RunsTheComplexScenario : IClassFixture<UseCases.ExecutionContextUseCase>
{
    private readonly UseCases.ExecutionContextUseCase useCase;
    private global::LowlandTech.TinyTools.ExecutionContext parent = null!;

    public SC09_RunsTheComplexScenario(UseCases.ExecutionContextUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a parent context with a model, an output path, a global variable and a transform service")]
    private void Given()
    {
        parent = new global::LowlandTech.TinyTools.ExecutionContext
        {
            Key = "parent-context",
            Model = new { Name = "TestModel", Version = "1.0" },
            OutputPath = @"C:\output"
        };
        parent.Set("GlobalVar", "global-value");
        parent.RegisterService("transform", input => input?.ToString()?.ToUpper());
    }

    [When("a child is created, populated, cursored once and merged back into its parent")]
    private void When()
    {
        var child = parent.CreateChild("child-context");
        child.Set("LocalVar", "local-value");
        child.Set("Result", child.Services("transform")("hello"));
        child.PushCursor(new { Index = 0 }, 0);
        child.PushCursor(new { Index = 1 }, 1);
        useCase.Cases["parent"] = parent;
        useCase.Cases["ChildParent"] = child.Parent;
        useCase.Cases["GlobalVar"] = child.Get("GlobalVar");
        useCase.Cases["LocalVar"] = child.Get("LocalVar");
        useCase.Cases["Result"] = child.Get("Result");
        useCase.Cases["CurrentIndex"] = child.CurrentIndex;
        useCase.Cases["CursorDepth"] = child.CursorDepth;
        parent.Merge(child, "Result", "LocalVar");
        useCase.Cases["MergedResult"] = parent.Get("Result");
        useCase.Cases["MergedLocalVar"] = parent.Get("LocalVar");
    }

    [Then("the child parent should be parent")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_LinksTheChildToItsParent() => useCase.Cases["ChildParent"].Should().BeSameAs(useCase.Cases["parent"]);

    [Then("the global variable should be global-value")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_InheritsTheGlobalVariable() => useCase.Cases["GlobalVar"].Should().Be("global-value");

    [Then("the local variable should be local-value")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_KeepsTheLocalVariable() => useCase.Cases["LocalVar"].Should().Be("local-value");

    [Then("the transformed result should be HELLO")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_TransformsThroughTheService() => useCase.Cases["Result"].Should().Be("HELLO");

    [Then("the child index should be 1")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_TracksTheChildIndex() => useCase.Cases["CurrentIndex"].Should().Be(1);

    [Then("the child depth should be 2")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_TracksTheChildDepth() => useCase.Cases["CursorDepth"].Should().Be(2);

    [Then("the merged result should be HELLO")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_MergesTheResultBack() => useCase.Cases["MergedResult"].Should().Be("HELLO");

    [Then("the merged local variable should be local-value")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_MergesTheLocalVariableBack() => useCase.Cases["MergedLocalVar"].Should().Be("local-value");
}
