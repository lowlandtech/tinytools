namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

/// <summary>
/// Tests TemplateBase with JSON string data.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "08")]
[UserStory("03", "Real-world scenarios generate practical output")]
public class WhenRenderingTemplateFromJsonString : TinyToolsScenario<ComponentTemplate>
{
    private TemplateResult? _result;
    private string? _jsonData;

    protected override ComponentTemplate For()
    {
        return new ComponentTemplate();
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _jsonData = @"{
            ""ComponentName"": ""JsonComponent"",
            ""Props"": [
                { ""Name"": ""value"", ""Type"": ""number"" }
            ],
            ""PropsDestructured"": ""{ value }""
        }";
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        var data = JsonSerializer.Deserialize<ComponentData>(_jsonData!);
        _result = Sut.Render(data!);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Deserialize And Render Correctly")]
    [Fact]
    public void ItShouldDeserializeAndRenderCorrectly()
    {
        ArrangeAndAct();
        _result.Should().NotBeNull();
        _result!.Content.Should().Contain("JsonComponent");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Contain Deserialized Prop")]
    [Fact]
    public void ItShouldContainDeserializedProp()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("value: number");
    }
}
