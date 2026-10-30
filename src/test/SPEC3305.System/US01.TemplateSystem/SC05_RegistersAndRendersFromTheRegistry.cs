namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "05")]
[UserStory("01", "Template system validates and renders example templates")]
public sealed class SC05_RegistersAndRendersFromTheRegistry : TinyToolsScenario<TemplateRegistry>
{
    private ComponentTemplate _componentTemplate = null!;
    private CSharpClassTemplate _classTemplate = null!;
    private ComponentData _testComponentData = null!;
    private Dictionary<string, ComponentData> _batchData = null!;
    private ITemplate? _retrieved;
    private Dictionary<string, TemplateValidationResult> _validations = null!;
    private TemplateResult? _rendered;
    private Dictionary<string, TemplateResult> _batch = null!;

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
        _retrieved = Sut.Get("component");
        _validations = Sut.ValidateAll();
        _rendered = Sut.Render("component", _testComponentData);
        _batch = new();
        foreach (var (key, data) in _batchData)
        {
            var result = Sut.Render("component", data);
            if (result != null)
                _batch[key] = result;
        }
    }

    public SC05_RegistersAndRendersFromTheRegistry() => ArrangeAndAct();

    [Then("the retrieved template should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RetrievesTheRegisteredTemplate() => _retrieved.Should().NotBeNull();

    [Then("the retrieved template should be identical")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RetrievesTheSameInstance() => _retrieved.Should().BeSameAs(_componentTemplate);

    [Then("the validation results should have count 2")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ValidatesEveryRegisteredTemplate() => _validations.Should().HaveCount(2);

    [Then("every registered template validation should be accepted")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_AcceptsEveryRegisteredTemplate() => _validations.Values.Should().OnlyContain(validation => validation.IsValid);

    [Then("the rendered template should not be null")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RendersTheTemplateByName() => _rendered.Should().NotBeNull();

    [Then("the rendered component should contain TestComponent")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RendersTheRequestedComponentName() => _rendered!.Content.Should().Contain("TestComponent");

    [Then("the batch results should have count 2")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_RendersEveryBatchEntry() => _batch.Should().HaveCount(2);

    [Then("the first batch result should contain Button")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_RendersTheFirstBatchComponent() => _batch["component1"].Content.Should().Contain("Button");

    [Then("the second batch result should contain Input")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_RendersTheSecondBatchComponent() => _batch["component2"].Content.Should().Contain("Input");
}
