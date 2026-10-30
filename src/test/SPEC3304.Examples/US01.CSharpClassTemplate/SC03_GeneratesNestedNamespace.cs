namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "03")]
[UserStory("01", "CSharp class template generates class code")]
public sealed class SC03_GeneratesNestedNamespace : UnitScenario
{
    private readonly Shared.Examples.CSharpClassTemplate _template = new();
    private ClassData _data = null!;
    private TemplateResult _result = null!;

    [Given("a product entity definition in a nested namespace")]
    protected override void Given() => _data = new ClassData
    {
        Namespace = "App.Domain.Models.Entities",
        ClassName = "Product",
        Description = "Represents a product entity",
        IncludeConstructor = false,
        Properties = new()
        {
            new PropertyData
            {
                Name = "ProductId",
                Type = "Guid",
                Description = "Product unique identifier",
                DefaultValue = "Guid.NewGuid()"
            }
        },
        Methods = new()
    };

    [When("the class template renders the definition")]
    protected override void When() => _result = _template.Render(_data);

    public SC03_GeneratesNestedNamespace() => ArrangeAndAct();

    [Then("the generated path should be src/App/Domain/Models/Entities/Product.cs")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_GeneratesTheNestedPath() => _result.Path.Should().Be("src/App/Domain/Models/Entities/Product.cs");

    [Then("the namespace should be App.Domain.Models.Entities")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_UsesTheFullNamespace() => _result.Namespace.Should().Be("App.Domain.Models.Entities");

    [Then("the content should contain the namespace declaration")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ContainsTheNamespaceInContent() => _result.Content.Should().Contain("namespace App.Domain.Models.Entities;");

    [Then("the content should not contain a constructor")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ContainsNoConstructor() => _result.Content.Should().NotContain("public Product()");

    [Then("the content should contain the defaulted product id property")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ContainsThePropertyWithDefaultValue() => _result.Content.Should().Contain("public Guid ProductId { get; set; } = Guid.NewGuid();");
}
