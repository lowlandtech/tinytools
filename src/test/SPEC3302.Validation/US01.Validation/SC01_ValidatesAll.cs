namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US01.Validation;

/// <summary>
/// Tests TemplateRegistry validation functionality.
/// </summary>
[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template validation checks all templates in registry")]
public class WhenValidatingAllTemplatesInRegistry : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult>? _validationResults;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        registry.Register("class", new CSharpClassTemplate());
        return registry;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _validationResults = Sut.ValidateAll();
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Validate All Templates")]
    [Fact]
    public void ItShouldValidateAllTemplates()
    {
        ArrangeAndAct();
        _validationResults.Should().NotBeNull();
        _validationResults.Should().HaveCount(2);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Have Component Validation")]
    [Fact]
    public void ItShouldHaveComponentValidation()
    {
        ArrangeAndAct();
        _validationResults.Should().ContainKey("component");
        _validationResults!["component"].IsValid.Should().BeTrue();
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Have Class Validation")]
    [Fact]
    public void ItShouldHaveClassValidation()
    {
        ArrangeAndAct();
        _validationResults.Should().ContainKey("class");
        _validationResults!["class"].IsValid.Should().BeTrue();
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Have No Errors")]
    [Fact]
    public void ItShouldHaveNoErrors()
    {
        ArrangeAndAct();
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
[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template validation checks all templates in registry")]
public class WhenValidatingNonTemplateBaseImplementations : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult>? _validationResults;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("simple-valid", new SimpleNonBaseTemplate(isValid: true));
        registry.Register("simple-invalid", new SimpleNonBaseTemplate(isValid: false));
        return registry;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _validationResults = Sut.ValidateAll();
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Validate Both Templates")]
    [Fact]
    public void ItShouldValidateBothTemplates()
    {
        ArrangeAndAct();
        _validationResults.Should().HaveCount(2);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Have Valid Result For Valid Template")]
    [Fact]
    public void ItShouldHaveValidResultForValidTemplate()
    {
        ArrangeAndAct();
        _validationResults.Should().ContainKey("simple-valid");
        _validationResults!["simple-valid"].IsValid.Should().BeTrue();
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Have No Error Message For Valid Template")]
    [Fact]
    public void ItShouldHaveNoErrorMessageForValidTemplate()
    {
        ArrangeAndAct();
        _validationResults!["simple-valid"].ErrorMessage.Should().BeNull();
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Have Invalid Result For Invalid Template")]
    [Fact]
    public void ItShouldHaveInvalidResultForInvalidTemplate()
    {
        ArrangeAndAct();
        _validationResults.Should().ContainKey("simple-invalid");
        _validationResults!["simple-invalid"].IsValid.Should().BeFalse();
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Have Error Message For Invalid Template")]
    [Fact]
    public void ItShouldHaveErrorMessageForInvalidTemplate()
    {
        ArrangeAndAct();
        _validationResults!["simple-invalid"].ErrorMessage.Should().Be("Validation failed");
    }
}

/// <summary>
/// Tests ValidateAll with mixed TemplateBase and non-TemplateBase implementations.
/// </summary>
[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template validation checks all templates in registry")]
public class WhenValidatingMixedTemplateTypes : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, TemplateValidationResult>? _validationResults;

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate()); // TemplateBase
        registry.Register("simple", new SimpleNonBaseTemplate()); // Non-TemplateBase
        return registry;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _validationResults = Sut.ValidateAll();
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Validate Both Types")]
    [Fact]
    public void ItShouldValidateBothTypes()
    {
        ArrangeAndAct();
        _validationResults.Should().HaveCount(2);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Validate Template Base Derived")]
    [Fact]
    public void ItShouldValidateTemplateBaseDerived()
    {
        ArrangeAndAct();
        _validationResults.Should().ContainKey("component");
        _validationResults!["component"].IsValid.Should().BeTrue();
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Validate Non Template Base")]
    [Fact]
    public void ItShouldValidateNonTemplateBase()
    {
        ArrangeAndAct();
        _validationResults.Should().ContainKey("simple");
        _validationResults!["simple"].IsValid.Should().BeTrue();
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Have Detailed Result For Template Base")]
    [Fact]
    public void ItShouldHaveDetailedResultForTemplateBase()
    {
        ArrangeAndAct();
        // TemplateBase derived templates get detailed validation with ActualResult
        _validationResults!["component"].ActualResult.Should().NotBeNull();
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Have No Detailed Result For Non Template Base")]
    [Fact]
    public void ItShouldHaveNoDetailedResultForNonTemplateBase()
    {
        ArrangeAndAct();
        // Non-TemplateBase templates don't get ActualResult
        _validationResults!["simple"].ActualResult.Should().BeNull();
    }
}
