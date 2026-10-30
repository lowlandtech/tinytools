namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "08")]
[UserStory("01", "Template system validates and renders example templates")]
public sealed class SC08_RendersComplexNestedData : TinyToolsScenario<CSharpClassTemplate>
{
    private ClassData _complexData = null!;
    private TemplateResult _rendered = null!;

    protected override CSharpClassTemplate For() => new();

    [Given("complex nested class data for the example template")]
    protected override void Given()
    {
        _complexData = new ClassData
        {
            Namespace = "App.Models.Domain",
            ClassName = "ComplexEntity",
            Description = "A complex entity",
            Properties = new()
            {
                new PropertyData { Name = "Prop1", Type = "string", Description = "First" },
                new PropertyData { Name = "Prop2", Type = "int", Description = "Second" },
                new PropertyData { Name = "Prop3", Type = "bool", Description = "Third" }
            }
        };
    }

    [When("the template is rendered with the complex nested data")]
    protected override void When()
    {
        base.When();
        _rendered = Sut.Render(_complexData);
    }

    public SC08_RendersComplexNestedData() => ArrangeAndAct();

    [Then("the rendered path should be src/App/Models/Domain/ComplexEntity.cs")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheNestedPath() => _rendered.Path.Should().Be("src/App/Models/Domain/ComplexEntity.cs");

    [Then("the rendered namespace should be App.Models.Domain")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheNestedNamespace() => _rendered.Namespace.Should().Be("App.Models.Domain");

    [Then("the rendered class should contain the domain namespace")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersTheNestedContent() => _rendered.Content.Should().Contain("namespace App.Models.Domain;");
}
