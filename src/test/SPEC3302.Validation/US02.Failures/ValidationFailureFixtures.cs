namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US02.Failures;

/// <summary>
/// Template whose expectations deliberately mismatch, so validation fails with
/// differences.
/// </summary>
public class InvalidTemplate : TemplateBase
{
    public override string TemplatePath => "test.txt";

    public override string TemplateNamespace => "Test";

    public override string TemplateContent => "Output: ${Context.Value}";

    public override Type DataType => typeof(TestData);

    public override string TestDataJson => @"{ ""Value"": ""Test"" }";

    public override string ExpectedContent => "This will NOT match!";

    public override string ExpectedPath => "wrong-path.txt";

    public override string ExpectedNamespace => "WrongNamespace";
}

public record TestData
{
    public string Value { get; init; } = "";
}

/// <summary>Template whose test JSON deserializes to null.</summary>
public class NullDeserializationTemplate : TemplateBase
{
    public override string TemplatePath => "test.txt";

    public override string TemplateNamespace => "Test";

    public override string TemplateContent => "Output: ${Context.Value}";

    public override Type DataType => typeof(NullableTestData);

    public override string TestDataJson => "null";

    public override string ExpectedContent => "Doesn't matter";
}

public class NullableTestData
{
    public string? Value { get; init; }
}

/// <summary>Template whose test JSON throws during deserialization.</summary>
public class ExceptionThrowingTemplate : TemplateBase
{
    public override string TemplatePath => "test.txt";

    public override string TemplateNamespace => "Test";

    public override string TemplateContent => "Output: ${Context.Value}";

    public override Type DataType => typeof(TestData);

    public override string TestDataJson => "{ invalid json }";

    public override string ExpectedContent => "Doesn't matter";
}
