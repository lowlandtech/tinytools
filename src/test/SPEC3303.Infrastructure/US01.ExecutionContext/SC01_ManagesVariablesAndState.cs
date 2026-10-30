namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US01.ExecutionContext;

[Trait("SPEC", "3303"), Trait("US", "01"), Trait("SC", "01")]
[UserStory("01", "Tool context manages variables and state")]
public sealed class SC01_ManagesVariablesAndState : IClassFixture<UseCases.ExecutionContextUseCase>
{
    private readonly UseCases.ExecutionContextUseCase useCase;
    private global::LowlandTech.TinyTools.ExecutionContext plain = null!;
    private global::LowlandTech.TinyTools.ExecutionContext named = null!;
    private global::LowlandTech.TinyTools.ExecutionContext parent = null!;
    private global::LowlandTech.TinyTools.ExecutionContext child = null!;
    private global::LowlandTech.TinyTools.ExecutionContext loaded = null!;
    private object model = null!;

    public SC01_ManagesVariablesAndState(UseCases.ExecutionContextUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a default context, a named context, a child context and a context with a model and an output path")]
    private void Given()
    {
        plain = new global::LowlandTech.TinyTools.ExecutionContext();
        named = new global::LowlandTech.TinyTools.ExecutionContext { Key = "test-context" };
        parent = new global::LowlandTech.TinyTools.ExecutionContext { Key = "parent" };
        child = parent.CreateChild("child");
        model = new { Name = "Test", Value = 42 };
        loaded = new global::LowlandTech.TinyTools.ExecutionContext { Model = model, OutputPath = @"C:\output\path" };
    }

    [When("every public member is read once from the context that owns it")]
    private void When()
    {
        useCase.Cases["Key"] = named.Key;
        useCase.Cases["DefaultKey"] = plain.Key;
        useCase.Cases["DefaultParent"] = plain.Parent;
        useCase.Cases["parent"] = parent;
        useCase.Cases["ChildParent"] = child.Parent;
        useCase.Cases["ChildKey"] = child.Key;
        useCase.Cases["model"] = model;
        useCase.Cases["Model"] = loaded.Model;
        useCase.Cases["ModelVariable"] = loaded.Get("Model");
        useCase.Cases["DefaultModel"] = plain.Model;
        useCase.Cases["OutputPath"] = loaded.OutputPath;
        useCase.Cases["OutputPathVariable"] = loaded.Get("OutputPath");
        useCase.Cases["DefaultOutputPath"] = plain.OutputPath;
    }

    [Then("the context key should be test-context")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_SetsTheKey() => useCase.Cases["Key"].Should().Be("test-context");

    [Then("the default key should be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_DefaultsTheKey() => useCase.Cases["DefaultKey"].Should().BeNull();

    [Then("the default parent should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_DefaultsTheParent() => useCase.Cases["DefaultParent"].Should().BeNull();

    [Then("the child parent should be parent")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_SetsTheChildParent() => useCase.Cases["ChildParent"].Should().BeSameAs(useCase.Cases["parent"]);

    [Then("the child key should be child")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_SetsTheChildKey() => useCase.Cases["ChildKey"].Should().Be("child");

    [Then("the context model should be model")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_SetsAndGetsTheModel() => useCase.Cases["Model"].Should().BeSameAs(useCase.Cases["model"]);

    [Then("the model variable should be model")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ExposesTheModelAsAVariable() => useCase.Cases["ModelVariable"].Should().BeSameAs(useCase.Cases["model"]);

    [Then("the default model should be null")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_DefaultsTheModel() => useCase.Cases["DefaultModel"].Should().BeNull();

    [Then("the output path should be C:\\output\\path")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_SetsAndGetsTheOutputPath() => useCase.Cases["OutputPath"].Should().Be(@"C:\output\path");

    [Then("the output path variable should be C:\\output\\path")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_ExposesTheOutputPathAsAVariable() => useCase.Cases["OutputPathVariable"].Should().Be(@"C:\output\path");

    [Then("the default output path should be null")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_DefaultsTheOutputPath() => useCase.Cases["DefaultOutputPath"].Should().BeNull();
}
