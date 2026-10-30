namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "04")]
[UserStory("01", "Template system validates and renders example templates")]
public sealed class SC04_ValidatesAndRendersCSharpClasses : TinyToolsScenario<CSharpClassTemplate>
{
    private ClassData _productData = null!;
    private TemplateValidationResult _validation = null!;
    private TemplateResult _rendered = null!;

    protected override CSharpClassTemplate For() => new();

    [Given("the example C# class template and the data of each case")]
    protected override void Given()
    {
        _productData = new ClassData
        {
            Namespace = "MyApp.Models",
            ClassName = "Product",
            Description = "Represents a product",
            IncludeConstructor = true,
            Properties = new()
            {
                new PropertyData
                {
                    Name = "Id",
                    Type = "int",
                    Description = "Product identifier"
                },
                new PropertyData
                {
                    Name = "Name",
                    Type = "string",
                    Description = "Product name",
                    DefaultValue = "\"\""
                }
            },
            Methods = new()
            {
                new MethodData
                {
                    Name = "Validate",
                    ReturnType = "bool",
                    Parameters = "",
                    Description = "Validates the product"
                }
            }
        };
    }

    [When("the template is validated and rendered for each case")]
    protected override void When()
    {
        base.When();
        _validation = Sut.ValidateDetailed();
        _rendered = Sut.Render(_productData);
    }

    public SC04_ValidatesAndRendersCSharpClasses() => ArrangeAndAct();

    [Then("the C# class template should be accepted")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ValidatesTheCSharpClassTemplate() => _validation.IsValid.Should().BeTrue(_validation.ErrorMessage ?? "Validation should pass");

    [Then("the rendered class should contain the MyApp.Models namespace")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheClassNamespace() => _rendered.Content.Should().Contain("namespace MyApp.Models;");

    [Then("the rendered class should contain the Product class declaration")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersTheClassDeclaration() => _rendered.Content.Should().Contain("public class Product");

    [Then("the rendered class should contain the Id property")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersTheIdProperty() => _rendered.Content.Should().Contain("public int Id { get; set; }");

    [Then("the rendered class should contain the Name property default")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RendersTheNamePropertyDefault() => _rendered.Content.Should().Contain("public string Name { get; set; } = \"\";");

    [Then("the rendered class should contain the Product constructor")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RendersTheConstructor() => _rendered.Content.Should().Contain("public Product()");

    [Then("the rendered class should contain the Validate method")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_RendersTheValidateMethod() => _rendered.Content.Should().Contain("public bool Validate()");
}
