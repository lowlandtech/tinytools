namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US01.Validation;

/// <summary>
/// Template with partial expectations: path and namespace are absent, so only the
/// content may be validated.
/// </summary>
public class PartialExpectationTemplate : TemplateBase
{
    public override string TemplatePath => "output/${Context.Name}.txt";

    public override string TemplateNamespace => "Test.${Context.Name}";

    public override string TemplateContent => "Value: ${Context.Name}";

    public override Type DataType => typeof(SimpleData);

    public override string TestDataJson => @"{ ""Name"": ""Test"" }";

    public override string ExpectedContent => "Value: Test";
}

public record SimpleData
{
    public string Name { get; init; } = "";
}
