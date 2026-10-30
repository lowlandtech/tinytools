namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US03.BatchRendering;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "06")]
[UserStory("03", "Template registry renders batches of templates")]
public sealed class SC06_RendersSingleTemplateBatch : TinyToolsScenario<TemplateRegistry>
{
    private Dictionary<string, object> _batch = [];
    private Dictionary<string, TemplateResult> _results = [];

    protected override TemplateRegistry For()
    {
        var registry = new TemplateRegistry();
        registry.Register("class", new CSharpClassTemplate());
        return registry;
    }

    [Given("a class model describing an OrderService with an Id property")]
    protected override void Given() => _batch = new Dictionary<string, object>
    {
        ["class"] = new ClassData
        {
            Namespace = "MyApp.Services",
            ClassName = "OrderService",
            Description = "Handles order operations",
            Properties = new()
            {
                new PropertyData { Name = "Id", Type = "int", Description = "Order ID" },
            },
        },
    };

    [When("the single class model is rendered in one batch")]
    protected override void When()
    {
        base.When();

        _results = Sut.RenderBatch(_batch);
    }

    public SC06_RendersSingleTemplateBatch() => ArrangeAndAct();

    [Then("the batch results should have count 1")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ReturnsSingleResult() => _results.Should().HaveCount(1);

    [Then("the class content should contain OrderService")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersClassName() => _results["class"].Content.Should().Contain("OrderService");

    [Then("the class content should contain int Id")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersClassProperty() => _results["class"].Content.Should().Contain("int Id");

    [Then("the class namespace should be MyApp.Services")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HasCorrectNamespace() => _results["class"].Namespace.Should().Be("MyApp.Services");
}
