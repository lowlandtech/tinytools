using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests TemplateRegistry with failed validation.
/// </summary>
public class WhenValidatingTemplateWithFailures : WhenTestingFor<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult>? _validationResults;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("valid", new ComponentTemplate());
        registry.Register("invalid", new InvalidTemplate());
        return registry;
    }

    protected override void When()
    {
        _validationResults = Sut.ValidateAll();
    }

    [Fact]
    public void ItShouldHaveValidTemplatePass()
    {
        _validationResults!["valid"].IsValid.Should().BeTrue();
    }

    [Fact]
    public void ItShouldHaveInvalidTemplateFail()
    {
        _validationResults!["invalid"].IsValid.Should().BeFalse();
    }

    [Fact]
    public void ItShouldHaveErrorMessageForInvalidTemplate()
    {
        _validationResults!["invalid"].ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ItShouldHaveDifferencesForInvalidTemplate()
    {
        _validationResults!["invalid"].Differences.Should().NotBeNullOrEmpty();
    }
}

/// <summary>
/// Invalid template for testing validation failures.
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

/// <summary>
/// Template that returns null when TestDataJson is deserialized.
/// Used to test the null deserialization branch in ValidateDetailed.
/// </summary>
public class NullDeserializationTemplate : TemplateBase
{
    public override string TemplatePath => "test.txt";
    public override string TemplateNamespace => "Test";
    public override string TemplateContent => "Output: ${Context.Value}";
    public override Type DataType => typeof(NullableTestData);
    
    // This JSON deserializes to null for NullableTestData
    public override string TestDataJson => "null";
    public override string ExpectedContent => "Doesn't matter";
}

public class NullableTestData
{
    public string? Value { get; init; }
}

/// <summary>
/// Tests ValidateDetailed when TestDataJson deserializes to null.
/// </summary>
public class WhenValidatingTemplateWithNullDeserializationResult : WhenTestingFor<NullDeserializationTemplate>
{
    private TemplateValidationResult? _result;

    protected override NullDeserializationTemplate For()
    {
        return new NullDeserializationTemplate();
    }

    protected override void When()
    {
        _result = Sut.ValidateDetailed();
    }

    [Fact]
    public void ItShouldReturnInvalidResult()
    {
        _result!.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ItShouldHaveDeserializationErrorMessage()
    {
        _result!.ErrorMessage.Should().Contain("Failed to deserialize TestDataJson");
    }

    [Fact]
    public void ItShouldHaveNoActualResult()
    {
        _result!.ActualResult.Should().BeNull();
    }
}

/// <summary>
/// Template that causes an exception during validation.
/// Used to test the catch block in ValidateDetailed.
/// </summary>
public class ExceptionThrowingTemplate : TemplateBase
{
    public override string TemplatePath => "test.txt";
    public override string TemplateNamespace => "Test";
    public override string TemplateContent => "Output: ${Context.Value}";
    public override Type DataType => typeof(TestData);
    
    // Invalid JSON that will throw during deserialization
    public override string TestDataJson => "{ invalid json }";
    public override string ExpectedContent => "Doesn't matter";
}

/// <summary>
/// Tests ValidateDetailed when an exception is thrown during validation.
/// </summary>
public class WhenValidatingTemplateWithException : WhenTestingFor<ExceptionThrowingTemplate>
{
    private TemplateValidationResult? _result;

    protected override ExceptionThrowingTemplate For()
    {
        return new ExceptionThrowingTemplate();
    }

    protected override void When()
    {
        _result = Sut.ValidateDetailed();
    }

    [Fact]
    public void ItShouldReturnInvalidResult()
    {
        _result!.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ItShouldHaveExceptionErrorMessage()
    {
        _result!.ErrorMessage.Should().Contain("Validation threw exception:");
    }

    [Fact]
    public void ItShouldHaveNoActualResult()
    {
        _result!.ActualResult.Should().BeNull();
    }

    [Fact]
    public void ItShouldNotThrowException()
    {
        // The validation should catch the exception and return a result, not throw
        var action = () => Sut.ValidateDetailed();
        action.Should().NotThrow();
    }
}
