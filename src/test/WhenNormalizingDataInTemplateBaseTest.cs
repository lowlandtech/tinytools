using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests TemplateBase data normalization.
/// </summary>
public class WhenNormalizingDataInTemplateBase : WhenTestingFor<ComponentTemplate>
{
    private TemplateResult? _result;
    private object? _anonymousData;

    protected override ComponentTemplate For()
    {
        return new ComponentTemplate();
    }

    protected override void Given()
    {
        // Create anonymous object that matches ComponentData structure
        _anonymousData = new
        {
            ComponentName = "TestComponent",
            Props = new object[]
            {
                new { Name = "prop1", Type = "string" }
            },
            PropsDestructured = "{ prop1 }"
        };
    }

    protected override void When()
    {
        _result = Sut.Render(_anonymousData!);
    }

    [Fact]
    public void ItShouldNormalizeAnonymousDataSuccessfully()
    {
        _result.Should().NotBeNull();
    }

    [Fact]
    public void ItShouldRenderWithNormalizedData()
    {
        _result!.Content.Should().Contain("TestComponent");
        _result.Content.Should().Contain("prop1");
    }

    [Fact]
    public void ItShouldGenerateCorrectPath()
    {
        _result!.Path.Should().Be("src/components/TestComponent.tsx");
    }
}
