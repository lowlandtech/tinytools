namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

/// <summary>
/// Test template that deliberately fails validation for testing purposes.
/// </summary>
public class TestFailingTemplate : TemplateBase
{
    public override string TemplatePath => "test.txt";
    public override string TemplateNamespace => "Test";
    public override string TemplateContent => "Wrong content: ${Context.Value}";
    public override Type DataType => typeof(TestData);

    public override string TestDataJson => @"{ ""Value"": ""Test"" }";
    public override string ExpectedContent => "This will not match!";
}
