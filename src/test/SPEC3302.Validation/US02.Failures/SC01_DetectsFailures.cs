namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US02.Failures;

/// <summary>
/// Tests TemplateRegistry with failed validation.
/// </summary>
[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Template validation handles failure scenarios")]
public class WhenValidatingTemplateWithFailures : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult>? _validationResults;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("valid", new ComponentTemplate());
        registry.Register("invalid", new InvalidTemplate());
        return registry;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _validationResults = Sut.ValidateAll();
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Have Valid Template Pass")]
    [Fact]
    public void ItShouldHaveValidTemplatePass()
    {
        ArrangeAndAct();
        _validationResults!["valid"].IsValid.Should().BeTrue();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Have Invalid Template Fail")]
    [Fact]
    public void ItShouldHaveInvalidTemplateFail()
    {
        ArrangeAndAct();
        _validationResults!["invalid"].IsValid.Should().BeFalse();
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Have Error Message For Invalid Template")]
    [Fact]
    public void ItShouldHaveErrorMessageForInvalidTemplate()
    {
        ArrangeAndAct();
        _validationResults!["invalid"].ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Have Differences For Invalid Template")]
    [Fact]
    public void ItShouldHaveDifferencesForInvalidTemplate()
    {
        ArrangeAndAct();
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

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Template validation handles failure scenarios")]
public class NullableTestData
{
    public string? Value { get; init; }
}

/// <summary>
/// Tests ValidateDetailed when TestDataJson deserializes to null.
/// </summary>
[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Template validation handles failure scenarios")]
public class WhenValidatingTemplateWithNullDeserializationResult : TinyToolsScenario<NullDeserializationTemplate>
{
    private TemplateValidationResult? _result;

    protected override NullDeserializationTemplate For()
    {
        return new NullDeserializationTemplate();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.ValidateDetailed();
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Return Invalid Result")]
    [Fact]
    public void ItShouldReturnInvalidResult()
    {
        ArrangeAndAct();
        _result!.IsValid.Should().BeFalse();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Have Deserialization Error Message")]
    [Fact]
    public void ItShouldHaveDeserializationErrorMessage()
    {
        ArrangeAndAct();
        _result!.ErrorMessage.Should().Contain("Failed to deserialize TestDataJson");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Have No Actual Result")]
    [Fact]
    public void ItShouldHaveNoActualResult()
    {
        ArrangeAndAct();
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
[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Template validation handles failure scenarios")]
public class WhenValidatingTemplateWithException : TinyToolsScenario<ExceptionThrowingTemplate>
{
    private TemplateValidationResult? _result;

    protected override ExceptionThrowingTemplate For()
    {
        return new ExceptionThrowingTemplate();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.ValidateDetailed();
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Return Invalid Result")]
    [Fact]
    public void ItShouldReturnInvalidResult()
    {
        ArrangeAndAct();
        _result!.IsValid.Should().BeFalse();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Have Exception Error Message")]
    [Fact]
    public void ItShouldHaveExceptionErrorMessage()
    {
        ArrangeAndAct();
        _result!.ErrorMessage.Should().Contain("Validation threw exception:");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Have No Actual Result")]
    [Fact]
    public void ItShouldHaveNoActualResult()
    {
        ArrangeAndAct();
        _result!.ActualResult.Should().BeNull();
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Not Throw Exception")]
    [Fact]
    public void ItShouldNotThrowException()
    {
        ArrangeAndAct();
        // The validation should catch the exception and return a result, not throw
        var action = () => Sut.ValidateDetailed();
        action.Should().NotThrow();
    }
}
