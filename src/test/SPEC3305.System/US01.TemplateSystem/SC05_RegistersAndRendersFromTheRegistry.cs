namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "05")]
[UserStory("01", "Template system validates and renders example templates")]
public sealed class SC05_RegistersAndRendersFromTheRegistry : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.TemplateSystemUseCase>
{
    private readonly UseCases.TemplateSystemUseCase useCase;
    private ComponentTemplate _componentTemplate = null!;
    private CSharpClassTemplate _classTemplate = null!;
    private ComponentData _testComponentData = null!;
    private Dictionary<string, ComponentData> _batchData = null!;

    public SC05_RegistersAndRendersFromTheRegistry(UseCases.TemplateSystemUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TemplateRegistry For() => new();

    [Given("the registry templates and the data of each case")]
    protected override void Given()
    {
        _componentTemplate = new ComponentTemplate();
        _classTemplate = new CSharpClassTemplate();
        _testComponentData = new ComponentData
        {
            ComponentName = "TestComponent",
            Props = new(),
            PropsDestructured = "{}"
        };
        _batchData = new()
        {
            ["component1"] = new ComponentData { ComponentName = "Button", Props = new(), PropsDestructured = "{}" },
            ["component2"] = new ComponentData { ComponentName = "Input", Props = new(), PropsDestructured = "{}" }
        };
    }

    [When("the registry registers, validates and renders each case")]
    protected override void When()
    {
        base.When();
        Sut.Register("component", _componentTemplate);
        Sut.Register("class", _classTemplate);
        useCase.Cases["ComponentTemplate"] = _componentTemplate;
        useCase.Cases["Retrieved"] = Sut.Get("component");
        useCase.Cases["Validations"] = Sut.ValidateAll();
        useCase.Cases["Rendered"] = Sut.Render("component", _testComponentData);
        var batch = new Dictionary<string, TemplateResult>();
        foreach (var (key, data) in _batchData)
        {
            var result = Sut.Render("component", data);
            if (result != null)
                batch[key] = result;
        }
        useCase.Cases["Batch"] = batch;
    }

    [Then("the retrieved template should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RetrievesTheRegisteredTemplate() => useCase.Cases["Retrieved"].Should().NotBeNull();

    [Then("the retrieved template should be identical")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RetrievesTheSameInstance() => useCase.Cases["Retrieved"].Should().BeSameAs(useCase.Cases["ComponentTemplate"]);

    [Then("the validation results should have count 2")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ValidatesEveryRegisteredTemplate() => ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Validations"]!).Should().HaveCount(2);

    [Then("every registered template validation should be accepted")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_AcceptsEveryRegisteredTemplate() => ((Dictionary<string, TemplateValidationResult>)useCase.Cases["Validations"]!).Values.Should().OnlyContain(validation => validation.IsValid);

    [Then("the rendered template should not be null")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RendersTheTemplateByName() => useCase.Cases["Rendered"].Should().NotBeNull();

    [Then("the rendered component should contain TestComponent")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RendersTheRequestedComponentName() => ((TemplateResult)useCase.Cases["Rendered"]!).Content.Should().Contain("TestComponent");

    [Then("the batch results should have count 2")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_RendersEveryBatchEntry() => ((Dictionary<string, TemplateResult>)useCase.Cases["Batch"]!).Should().HaveCount(2);

    [Then("the first batch result should contain Button")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_RendersTheFirstBatchComponent() => ((Dictionary<string, TemplateResult>)useCase.Cases["Batch"]!)["component1"].Content.Should().Contain("Button");

    [Then("the second batch result should contain Input")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_RendersTheSecondBatchComponent() => ((Dictionary<string, TemplateResult>)useCase.Cases["Batch"]!)["component2"].Content.Should().Contain("Input");
}
