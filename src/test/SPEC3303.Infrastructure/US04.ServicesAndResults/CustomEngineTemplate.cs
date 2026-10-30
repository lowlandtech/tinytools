namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US04.ServicesAndResults;

/// <summary>Template that accepts an explicit engine so a custom engine can be proven.</summary>
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
