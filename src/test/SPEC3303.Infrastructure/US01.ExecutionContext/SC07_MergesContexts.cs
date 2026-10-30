namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US01.ExecutionContext;

[Trait("SPEC", "3303"), Trait("US", "01"), Trait("SC", "07")]
[UserStory("01", "Tool context manages variables and state")]
public sealed class SC07_MergesContexts : IClassFixture<UseCases.ExecutionContextUseCase>
{
    private readonly UseCases.ExecutionContextUseCase useCase;
    private global::LowlandTech.TinyTools.ExecutionContext target = null!;
    private global::LowlandTech.TinyTools.ExecutionContext source = null!;
    private global::LowlandTech.TinyTools.ExecutionContext sparseTarget = null!;
    private global::LowlandTech.TinyTools.ExecutionContext sparseSource = null!;
    private global::LowlandTech.TinyTools.ExecutionContext keptTarget = null!;
    private global::LowlandTech.TinyTools.ExecutionContext keptSource = null!;

    public SC07_MergesContexts(UseCases.ExecutionContextUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a merging pair, a pair carrying a missing key and a pair merged with no keys")]
    private void Given()
    {
        target = new global::LowlandTech.TinyTools.ExecutionContext();
        target.Set("Key1", "Value1");
        target.Set("Key2", "Value2");
        source = new global::LowlandTech.TinyTools.ExecutionContext();
        source.Set("Key2", "UpdatedValue2");
        source.Set("Key3", "Value3");
        sparseTarget = new global::LowlandTech.TinyTools.ExecutionContext();
        sparseSource = new global::LowlandTech.TinyTools.ExecutionContext();
        sparseSource.Set("ExistingKey", "value");
        keptTarget = new global::LowlandTech.TinyTools.ExecutionContext();
        keptTarget.Set("Key1", "Value1");
        keptSource = new global::LowlandTech.TinyTools.ExecutionContext();
        keptSource.Set("Key2", "Value2");
    }

    [When("each pair is merged once with its own key selection")]
    private void When()
    {
        target.Merge(source, "Key2", "Key3");
        useCase.Cases["Key1"] = target.Get("Key1");
        useCase.Cases["Key2"] = target.Get("Key2");
        useCase.Cases["Key3"] = target.Get("Key3");
        sparseTarget.Merge(sparseSource, "ExistingKey", "NonExistentKey");
        useCase.Cases["ExistingKey"] = sparseTarget.Get("ExistingKey");
        useCase.Cases["HasNonExistent"] = sparseTarget.Has("NonExistentKey");
        keptTarget.Merge(keptSource);
        useCase.Cases["KeptKey1"] = keptTarget.Get("Key1");
        useCase.Cases["HasKeptKey2"] = keptTarget.Has("Key2");
    }

    [Then("the first key should be Value1")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_KeepsAnUnmergedKey() => useCase.Cases["Key1"].Should().Be("Value1");

    [Then("the merged key should be UpdatedValue2")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_MergesAnExistingKey() => useCase.Cases["Key2"].Should().Be("UpdatedValue2");

    [Then("the added key should be Value3")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_AddsAMergedKey() => useCase.Cases["Key3"].Should().Be("Value3");

    [Then("the existing key should be value")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_MergesAnExistingSparseKey() => useCase.Cases["ExistingKey"].Should().Be("value");

    [Then("the non existing key should be false")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_IgnoresANonExistentKey() => useCase.Cases["HasNonExistent"].Should().Be(false);

    [Then("the kept key should be Value1")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_KeepsTheKeyWithoutSelection() => useCase.Cases["KeptKey1"].Should().Be("Value1");

    [Then("the unmerged key should be false")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_MergesNothingWithoutSelection() => useCase.Cases["HasKeptKey2"].Should().Be(false);
}
