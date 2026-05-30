using System.Text.Json;
using LowlandTech.TinyTools.Tests.Shared.Examples;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

/// <summary>
/// Tests TemplateBase with JSON string data.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "08")]
[UserStory("03", "Real-world scenarios generate practical output")]
public class WhenRenderingTemplateFromJsonString : WhenTestingFor<ComponentTemplate>
{
    private TemplateResult? _result;
    private string? _jsonData;

    protected override ComponentTemplate For()
    {
        return new ComponentTemplate();
    }

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

    protected override void When()
    {
        var data = JsonSerializer.Deserialize<ComponentData>(_jsonData!);
        _result = Sut.Render(data!);
    }

    [Fact]
    public void ItShouldDeserializeAndRenderCorrectly()
    {
        _result.Should().NotBeNull();
        _result!.Content.Should().Contain("JsonComponent");
    }

    [Fact]
    public void ItShouldContainDeserializedProp()
    {
        _result!.Content.Should().Contain("value: number");
    }
}
