using FluentAssertions;
using System.Text.Json;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests TemplateBase with JSON string data.
/// </summary>
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
