namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US01.Validation;

/// <summary>
/// Tests template validation with partial expectations.
/// </summary>
[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "02")]
[UserStory("01", "Template validation checks all templates in registry")]
public class WhenValidatingTemplateWithPartialExpectations : TinyToolsScenario<PartialExpectationTemplate>
{
    private TemplateValidationResult? _result;

    protected override PartialExpectationTemplate For()
    {
        return new PartialExpectationTemplate();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.ValidateDetailed();
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Validate Content Only")]
    [Fact]
    public void ItShouldValidateContentOnly()
    {
        ArrangeAndAct();
        _result.Should().NotBeNull();
        _result!.IsValid.Should().BeTrue();
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Not Validate Path Or Namespace")]
    [Fact]
    public void ItShouldNotValidatePathOrNamespace()
    {
        ArrangeAndAct();
        // Since ExpectedPath and ExpectedNamespace are null,
        // they should not be validated
        _result!.Differences.Should().BeNullOrEmpty();
    }
}

/// <summary>
/// Template with partial expectations for testing.
/// </summary>
public class PartialExpectationTemplate : TemplateBase
{
    public override string TemplatePath => "output/${Context.Name}.txt";
    public override string TemplateNamespace => "Test.${Context.Name}";
    public override string TemplateContent => "Value: ${Context.Name}";
    public override Type DataType => typeof(SimpleData);
    
    public override string TestDataJson => @"{ ""Name"": ""Test"" }";
    public override string ExpectedContent => "Value: Test";
    // ExpectedPath and ExpectedNamespace are null - should not be validated
}

public record SimpleData
{
    public string Name { get; init; } = "";
}
