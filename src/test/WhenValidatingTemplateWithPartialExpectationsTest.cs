using FluentAssertions;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests template validation with partial expectations.
/// </summary>
public class WhenValidatingTemplateWithPartialExpectations : WhenTestingFor<PartialExpectationTemplate>
{
    private TemplateValidationResult? _result;

    protected override PartialExpectationTemplate For()
    {
        return new PartialExpectationTemplate();
    }

    protected override void When()
    {
        _result = Sut.ValidateDetailed();
    }

    [Fact]
    public void ItShouldValidateContentOnly()
    {
        _result.Should().NotBeNull();
        _result!.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ItShouldNotValidatePathOrNamespace()
    {
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
