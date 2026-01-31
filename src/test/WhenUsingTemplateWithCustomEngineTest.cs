using FluentAssertions;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests template base with custom engine.
/// </summary>
public class WhenUsingTemplateWithCustomEngine : WhenTestingFor<CustomEngineTemplate>
{
    private TemplateResult? _result;

    protected override CustomEngineTemplate For()
    {
        var customEngine = new TinyTemplateEngine();
        return new CustomEngineTemplate(customEngine);
    }

    protected override void When()
    {
        _result = Sut.Render(new SimpleData { Name = "Custom" });
    }

    [Fact]
    public void ItShouldRenderWithCustomEngine()
    {
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

