namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "08")]
[UserStory("03", "Real-world scenarios generate practical output")]
public sealed class SC08_RendersFromJsonString : UnitScenario
{
    private readonly ComponentTemplate _component = new();
    private ComponentData _data = null!;
    private TemplateResult _result = null!;

    [Given("component data deserialized from a JSON string")]
    protected override void Given() => _data = JsonSerializer.Deserialize<ComponentData>(@"{
            ""ComponentName"": ""JsonComponent"",
            ""Props"": [
                { ""Name"": ""value"", ""Type"": ""number"" }
            ],
            ""PropsDestructured"": ""{ value }""
        }")!;

    [When("the component template renders the deserialized data")]
    protected override void When() => _result = _component.Render(_data);

    public SC08_RendersFromJsonString() => ArrangeAndAct();

    [Then("the result should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_DeserializesTheJsonData() => _result.Should().NotBeNull();

    [Then("the content should contain JsonComponent")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheDeserializedComponentName() => _result.Content.Should().Contain("JsonComponent");

    [Then("the content should contain the deserialized prop")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersTheDeserializedProp() => _result.Content.Should().Contain("value: number");
}
