namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "08")]
[UserStory("03", "Real-world scenarios generate practical output")]
public sealed class SC08_RendersFromJsonString : UnitScenario, IClassFixture<UseCases.RealWorldScenariosUseCase>
{
    private readonly UseCases.RealWorldScenariosUseCase useCase;
    private readonly ComponentTemplate _component = new();
    private ComponentData _data = null!;

    public SC08_RendersFromJsonString(UseCases.RealWorldScenariosUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("component data deserialized from a JSON string")]
    protected override void Given() => _data = JsonSerializer.Deserialize<ComponentData>(@"{
            ""ComponentName"": ""JsonComponent"",
            ""Props"": [
                { ""Name"": ""value"", ""Type"": ""number"" }
            ],
            ""PropsDestructured"": ""{ value }""
        }")!;

    [When("the component template renders the deserialized data")]
    protected override void When() => useCase.Cases["Result"] = _component.Render(_data);

    [Then("the result should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_DeserializesTheJsonData() => useCase.Cases["Result"].Should().NotBeNull();

    [Then("the content should contain JsonComponent")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheDeserializedComponentName() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().Contain("JsonComponent");

    [Then("the content should contain the deserialized prop")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersTheDeserializedProp() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().Contain("value: number");
}
