using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests TemplateRegistry validation functionality.
/// </summary>
public class WhenValidatingAllTemplatesInRegistry : WhenTestingFor<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult>? _validationResults;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        registry.Register("class", new CSharpClassTemplate());
        return registry;
    }

    protected override void When()
    {
        _validationResults = Sut.ValidateAll();
    }

    [Fact]
    public void ItShouldValidateAllTemplates()
    {
        _validationResults.Should().NotBeNull();
        _validationResults.Should().HaveCount(2);
    }

    [Fact]
    public void ItShouldHaveComponentValidation()
    {
        _validationResults.Should().ContainKey("component");
        _validationResults!["component"].IsValid.Should().BeTrue();
    }

    [Fact]
    public void ItShouldHaveClassValidation()
    {
        _validationResults.Should().ContainKey("class");
        _validationResults!["class"].IsValid.Should().BeTrue();
    }

    [Fact]
    public void ItShouldHaveNoErrors()
    {
        foreach (var result in _validationResults!.Values)
        {
            result.ErrorMessage.Should().BeNullOrEmpty();
        }
    }
}

/// <summary>
/// A simple ITemplate implementation that does NOT inherit from TemplateBase.
/// Used to test the else branch in ValidateAll().
/// </summary>
public class SimpleNonBaseTemplate : ITemplate
{
    private readonly bool _isValid;

    public SimpleNonBaseTemplate(bool isValid = true)
    {
        _isValid = isValid;
    }

    public string TemplatePath => "templates/simple.txt";
    public string TemplateNamespace => "Simple";
    public string TemplateContent => "Hello ${Context.Name}!";
    public Type DataType => typeof(object);
    public string TestDataJson => "{}";
    public string ExpectedContent => "Hello World!";
    public string? ExpectedPath => "output.txt";
    public string? ExpectedNamespace => "Simple";

    public TemplateResult Render(object data)
    {
        return new TemplateResult
        {
            Content = "Hello World!",
            Path = "output.txt",
            Namespace = "Simple"
        };
    }

    public bool Validate() => _isValid;
}

/// <summary>
/// Tests ValidateAll with non-TemplateBase ITemplate implementations (tests the else branch).
/// </summary>
public class WhenValidatingNonTemplateBaseImplementations : WhenTestingFor<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult>? _validationResults;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("simple-valid", new SimpleNonBaseTemplate(isValid: true));
        registry.Register("simple-invalid", new SimpleNonBaseTemplate(isValid: false));
        return registry;
    }

    protected override void When()
    {
        _validationResults = Sut.ValidateAll();
    }

    [Fact]
    public void ItShouldValidateBothTemplates()
    {
        _validationResults.Should().HaveCount(2);
    }

    [Fact]
    public void ItShouldHaveValidResultForValidTemplate()
    {
        _validationResults.Should().ContainKey("simple-valid");
        _validationResults!["simple-valid"].IsValid.Should().BeTrue();
    }

    [Fact]
    public void ItShouldHaveNoErrorMessageForValidTemplate()
    {
        _validationResults!["simple-valid"].ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ItShouldHaveInvalidResultForInvalidTemplate()
    {
        _validationResults.Should().ContainKey("simple-invalid");
        _validationResults!["simple-invalid"].IsValid.Should().BeFalse();
    }

    [Fact]
    public void ItShouldHaveErrorMessageForInvalidTemplate()
    {
        _validationResults!["simple-invalid"].ErrorMessage.Should().Be("Validation failed");
    }
}

/// <summary>
/// Tests ValidateAll with mixed TemplateBase and non-TemplateBase implementations.
/// </summary>
public class WhenValidatingMixedTemplateTypes : WhenTestingFor<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult>? _validationResults;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate()); // TemplateBase
        registry.Register("simple", new SimpleNonBaseTemplate()); // Non-TemplateBase
        return registry;
    }

    protected override void When()
    {
        _validationResults = Sut.ValidateAll();
    }

    [Fact]
    public void ItShouldValidateBothTypes()
    {
        _validationResults.Should().HaveCount(2);
    }

    [Fact]
    public void ItShouldValidateTemplateBaseDerived()
    {
        _validationResults.Should().ContainKey("component");
        _validationResults!["component"].IsValid.Should().BeTrue();
    }

    [Fact]
    public void ItShouldValidateNonTemplateBase()
    {
        _validationResults.Should().ContainKey("simple");
        _validationResults!["simple"].IsValid.Should().BeTrue();
    }

    [Fact]
    public void ItShouldHaveDetailedResultForTemplateBase()
    {
        // TemplateBase derived templates get detailed validation with ActualResult
        _validationResults!["component"].ActualResult.Should().NotBeNull();
    }

    [Fact]
    public void ItShouldHaveNoDetailedResultForNonTemplateBase()
    {
        // Non-TemplateBase templates don't get ActualResult
        _validationResults!["simple"].ActualResult.Should().BeNull();
    }
}
