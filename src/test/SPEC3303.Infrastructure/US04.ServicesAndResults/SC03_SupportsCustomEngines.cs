namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US04.ServicesAndResults;

/// <summary>
/// Tests template base with custom engine.
/// </summary>
[Trait(Spec.SPEC, "3303")]
[Trait(Spec.SC, "03")]
[UserStory("04", "Template services support custom engines")]
public class WhenUsingTemplateWithCustomEngine : TinyToolsScenario<CustomEngineTemplate>
{
    private TemplateResult? _result;

    protected override CustomEngineTemplate For()
    {
        var customEngine = new TinyTemplateEngine();
        return new CustomEngineTemplate(customEngine);
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(new SimpleData { Name = "Custom" });
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Render With Custom Engine")]
    [Fact]
    public void ItShouldRenderWithCustomEngine()
    {
        ArrangeAndAct();
        _result.Should().NotBeNull();
        _result!.Content.Should().Contain("Custom");
    }
}

/// <summary>
/// Template that accepts custom engine for testing.
/// </summary>
public class CustomEngineTemplate : TemplateBase
{
    public CustomEngineTemplate(ITemplateEngine engine) : base(engine)
    {
    }

    public override string TemplatePath => "test.txt";
    public override string TemplateNamespace => "Test";
    public override string TemplateContent => "Name: ${Context.Name}";
    public override Type DataType => typeof(SimpleData);
}

public record SimpleData
{
    public string Name { get; set; } = string.Empty;
}
