using LowlandTech.TinyTools.Tests.Shared.Examples;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

/// <summary>
/// Tests TemplateBase data normalization.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "05")]
[UserStory("01", "CSharp class template generates class code")]
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
