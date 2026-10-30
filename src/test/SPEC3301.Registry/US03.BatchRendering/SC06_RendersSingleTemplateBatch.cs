namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US03.BatchRendering;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "06")]
[UserStory("03", "Template registry renders batches of templates")]
public sealed class SC06_RendersSingleTemplateBatch : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.BatchRenderingUseCase>
{
    private readonly UseCases.BatchRenderingUseCase useCase;
    private Dictionary<string, object> _batch = [];

    public SC06_RendersSingleTemplateBatch(UseCases.BatchRenderingUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["Results"] = Sut.RenderBatch(_batch);
    }

    [Then("the batch results should have count 1")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ReturnsSingleResult() => ((Dictionary<string, TemplateResult>)useCase.Cases["Results"]!).Should().HaveCount(1);

    [Then("the class content should contain OrderService")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersClassName() => ((Dictionary<string, TemplateResult>)useCase.Cases["Results"]!)["class"].Content.Should().Contain("OrderService");

    [Then("the class content should contain int Id")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersClassProperty() => ((Dictionary<string, TemplateResult>)useCase.Cases["Results"]!)["class"].Content.Should().Contain("int Id");

    [Then("the class namespace should be MyApp.Services")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HasCorrectNamespace() => ((Dictionary<string, TemplateResult>)useCase.Cases["Results"]!)["class"].Namespace.Should().Be("MyApp.Services");
}
