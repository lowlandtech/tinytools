namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US01.ExecutionContext;

[Trait("SPEC", "3303"), Trait("US", "01"), Trait("SC", "06")]
[UserStory("01", "Tool context manages variables and state")]
public sealed class SC06_InheritsChildContexts : IClassFixture<UseCases.ExecutionContextUseCase>
{
    private readonly UseCases.ExecutionContextUseCase useCase;
    private global::LowlandTech.TinyTools.ExecutionContext cased = null!;
    private global::LowlandTech.TinyTools.ExecutionContext parent = null!;
    private global::LowlandTech.TinyTools.ExecutionContext child = null!;

    public SC06_InheritsChildContexts(UseCases.ExecutionContextUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a cased context and a parent carrying a name, a value and a transform service")]
    private void Given()
    {
        cased = new global::LowlandTech.TinyTools.ExecutionContext();
        cased.Set("TestKey", "value");
        parent = new global::LowlandTech.TinyTools.ExecutionContext();
        parent.Set("Name", "Parent");
        parent.Set("Value", 42);
        parent.RegisterService("upper", input => input?.ToString()?.ToUpper());
        child = parent.CreateChild();
    }

    [When("the child inherits its parent state and then overrides one variable")]
    private void When()
    {
        useCase.Cases["LowerKey"] = cased.Get("testkey");
        useCase.Cases["UpperKey"] = cased.Get("TESTKEY");
        useCase.Cases["MixedKey"] = cased.Get("TestKey");
        useCase.Cases["HasLowerKey"] = cased.Has("testkey");
        useCase.Cases["InheritedName"] = child.Get("Name");
        useCase.Cases["InheritedValue"] = child.Get("Value");
        useCase.Cases["InheritedService"] = child.Services("upper")("hello");
        child.Set("Name", "Child");
        child.Set("NewVar", "ChildOnly");
        useCase.Cases["ParentName"] = parent.Get("Name");
        useCase.Cases["ParentNewVar"] = parent.Has("NewVar");
        useCase.Cases["ChildName"] = child.Get("Name");
        useCase.Cases["ChildNewVar"] = child.Get("NewVar");
    }

    [Then("the lower key should be value")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ReadsALowerCaseKey() => useCase.Cases["LowerKey"].Should().Be("value");

    [Then("the upper key should be value")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ReadsAnUpperCaseKey() => useCase.Cases["UpperKey"].Should().Be("value");

    [Then("the mixed key should be value")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ReadsTheDeclaredKey() => useCase.Cases["MixedKey"].Should().Be("value");

    [Then("the lower key existence should be true")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ChecksALowerCaseKey() => useCase.Cases["HasLowerKey"].Should().Be(true);

    [Then("the inherited name should be Parent")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_InheritsTheName() => useCase.Cases["InheritedName"].Should().Be("Parent");

    [Then("the inherited value should be 42")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_InheritsTheValue() => useCase.Cases["InheritedValue"].Should().Be(42);

    [Then("the inherited service should be HELLO")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_InheritsTheService() => useCase.Cases["InheritedService"].Should().Be("HELLO");

    [Then("the parent name should be Parent")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_KeepsTheParentName() => useCase.Cases["ParentName"].Should().Be("Parent");

    [Then("the parent new variable should be false")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_KeepsTheParentVariables() => useCase.Cases["ParentNewVar"].Should().Be(false);

    [Then("the child name should be Child")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_OverridesTheChildName() => useCase.Cases["ChildName"].Should().Be("Child");

    [Then("the child new variable should be ChildOnly")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_AddsAChildVariable() => useCase.Cases["ChildNewVar"].Should().Be("ChildOnly");
}
