namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

/// <summary>
/// Tests TemplateBase data normalization.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "05")]
[UserStory("01", "CSharp class template generates class code")]
public class WhenNormalizingDataInTemplateBase : TinyToolsScenario<ComponentTemplate>
{
    private TemplateResult? _result;
    private object? _anonymousData;

    protected override ComponentTemplate For()
    {
        return new ComponentTemplate();
    }

    [Given("Setup test context")]
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_anonymousData!);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Normalize Anonymous Data Successfully")]
    [Fact]
    public void ItShouldNormalizeAnonymousDataSuccessfully()
    {
        ArrangeAndAct();
        _result.Should().NotBeNull();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Render With Normalized Data")]
    [Fact]
    public void ItShouldRenderWithNormalizedData()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("TestComponent");
        _result.Content.Should().Contain("prop1");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Generate Correct Path")]
    [Fact]
    public void ItShouldGenerateCorrectPath()
    {
        ArrangeAndAct();
        _result!.Path.Should().Be("src/components/TestComponent.tsx");
    }
}
