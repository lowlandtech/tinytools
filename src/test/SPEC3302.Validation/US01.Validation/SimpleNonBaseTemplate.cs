namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US01.Validation;

/// <summary>
/// A simple ITemplate implementation that does not inherit TemplateBase, used to
/// exercise the non-TemplateBase branch of ValidateAll.
/// </summary>
public class SimpleNonBaseTemplate : ITemplate
{
    private readonly bool _isValid;

    public SimpleNonBaseTemplate(bool isValid = true) => _isValid = isValid;

    public string TemplatePath => "templates/simple.txt";

    public string TemplateNamespace => "Simple";

    public string TemplateContent => "Hello ${Context.Name}!";

    public Type DataType => typeof(object);

    public string TestDataJson => "{}";

    public string ExpectedContent => "Hello World!";

    public string? ExpectedPath => "output.txt";

    public string? ExpectedNamespace => "Simple";

    public TemplateResult Render(object data) => new()
    {
        Content = "Hello World!",
        Path = "output.txt",
        Namespace = "Simple",
    };

    public bool Validate() => _isValid;
}
