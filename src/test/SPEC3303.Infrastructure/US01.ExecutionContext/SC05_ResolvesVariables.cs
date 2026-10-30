namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US01.ExecutionContext;

[Trait("SPEC", "3303"), Trait("US", "01"), Trait("SC", "05")]
[UserStory("01", "Tool context manages variables and state")]
public sealed class SC05_ResolvesVariables : IClassFixture<UseCases.ExecutionContextUseCase>
{
    private readonly UseCases.ExecutionContextUseCase useCase;
    private global::LowlandTech.TinyTools.ExecutionContext sparse = null!;
    private global::LowlandTech.TinyTools.ExecutionContext keyed = null!;

    public SC05_ResolvesVariables(UseCases.ExecutionContextUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a context with one variable and a context with three variables")]
    private void Given()
    {
        sparse = new global::LowlandTech.TinyTools.ExecutionContext();
        sparse.Set("Existing", "value");
        keyed = new global::LowlandTech.TinyTools.ExecutionContext();
        keyed.Set("Key1", "Value1");
        keyed.Set("Key2", "Value2");
        keyed.Set("Key3", "Value3");
    }

    [When("variable existence and the variable keys are read once")]
    private void When()
    {
        useCase.Cases["HasExisting"] = sparse.Has("Existing");
        useCase.Cases["HasMissing"] = sparse.Has("NonExisting");
        useCase.Cases["Keys"] = keyed.Keys.ToList();
    }

    [Then("the existing variable should be true")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ChecksAnExistingVariable() => useCase.Cases["HasExisting"].Should().Be(true);

    [Then("the missing variable should be false")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ChecksAMissingVariable() => useCase.Cases["HasMissing"].Should().Be(false);

    [Then("the variable keys should have count 3")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ReturnsEveryKey() => ((IEnumerable<string>)useCase.Cases["Keys"]!).Should().HaveCount(3);

    [Then("the variable keys should contain Key1")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ContainsTheFirstKey() => ((IEnumerable<string>)useCase.Cases["Keys"]!).Should().Contain("Key1");

    [Then("the variable keys should contain Key2")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ContainsTheSecondKey() => ((IEnumerable<string>)useCase.Cases["Keys"]!).Should().Contain("Key2");

    [Then("the variable keys should contain Key3")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ContainsTheThirdKey() => ((IEnumerable<string>)useCase.Cases["Keys"]!).Should().Contain("Key3");
}
