namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US01.Validation;

[Trait(Spec.SPEC, "3302")]
[Trait(Spec.SC, "05")]
[UserStory("01", "Template validation checks all templates in registry")]
public sealed class SC05_ValidatesMixedTemplateTypes : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.ValidationUseCase>
{
    private readonly UseCases.ValidationUseCase useCase;

    public SC05_ValidatesMixedTemplateTypes(UseCases.ValidationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("component", new ComponentTemplate());
        registry.Register("simple", new SimpleNonBaseTemplate());
        return registry;
    }

    [Given("a registry holding a base and a non-base template")]
    protected override void Given()
    {
    }

    [When("every registered template is validated")]
    protected override void When()
    {
        base.When();
        useCase.Cases["Results"] = Sut.ValidateAll();
    }

    [Then("the validated templates should have count 2")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ValidatesBothTypes() => ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!).Should().HaveCount(2);

    [Then("the base template should be accepted")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ValidatesTemplateBaseDerived() => new { Has = ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!).ContainsKey("component"), Valid = ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!)["component"].IsValid }.Should().BeEquivalentTo(new { Has = true, Valid = true });

    [Then("the simple template should be accepted")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ValidatesNonTemplateBase() => new { Has = ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!).ContainsKey("simple"), Valid = ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!)["simple"].IsValid }.Should().BeEquivalentTo(new { Has = true, Valid = true });

    [Then("the base template result should not be null")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HasDetailedResultForTemplateBase() => ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!)["component"].ActualResult.Should().NotBeNull();

    [Then("the non base template result should be null")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_HasNoDetailedResultForNonTemplateBase() => ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Results"]!)["simple"].ActualResult.Should().BeNull();
}
