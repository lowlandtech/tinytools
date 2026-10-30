namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "04")]
[UserStory("01", "CSharp class template generates class code")]
public sealed class SC04_RendersWithTestData : UnitScenario
{
    private readonly Shared.Examples.CSharpClassTemplate _template = new();
    private ClassData _data = null!;
    private TemplateResult _result = null!;

    [Given("the shared example class data")]
    protected override void Given()
    {
        _data = new ClassData
        {
            Namespace = "MyApp.Models",
            ClassName = "User",
            Description = "Represents a user in the system",
            IncludeConstructor = true,
            Properties = new()
            {
                new PropertyData { Name = "Id", Type = "int", Description = "User identifier" },
                new PropertyData { Name = "Name", Type = "string", Description = "User name", DefaultValue = "\"\"" }
            },
            Methods = new()
            {
                new MethodData { Name = "Validate", ReturnType = "bool", Parameters = "", Description = "Validates the user data" }
            }
        };
    }

    [When("the class template renders the definition")]
    protected override void When() => _result = _template.Render(_data);

    public SC04_RendersWithTestData() => ArrangeAndAct();

    [Then("the result should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ReturnsAResult() => _result.Should().NotBeNull();

    [Then("the generated path should be src/MyApp/Models/User.cs")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_GeneratesTheCorrectPath() => _result.Path.Should().Be("src/MyApp/Models/User.cs");

    [Then("the namespace should be MyApp.Models")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_GeneratesTheCorrectNamespace() => _result.Namespace.Should().Be("MyApp.Models");

    [Then("the content should contain the namespace declaration")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ContainsTheNamespaceDeclaration() => _result.Content.Should().Contain("namespace MyApp.Models;");

    [Then("the content should contain the class declaration")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ContainsTheClassDeclaration() => _result.Content.Should().Contain("public class User");

    [Then("the content should contain the class description")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ContainsTheClassDescription() => _result.Content.Should().Contain("Represents a user in the system");

    [Then("the content should contain the id property")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ContainsTheIdProperty() => _result.Content.Should().Contain("public int Id { get; set; }");

    [Then("the content should contain the defaulted name property")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_ContainsTheNamePropertyWithDefault() => _result.Content.Should().Contain("public string Name { get; set; } = \"\";");

    [Then("the content should contain the constructor")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_ContainsTheConstructor() => _result.Content.Should().Contain("public User()");

    [Then("the content should contain the validate method")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_ContainsTheValidateMethod() => _result.Content.Should().Contain("public bool Validate()").And.Contain("throw new NotImplementedException();");

    [Then("the content should contain the property descriptions")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_ContainsThePropertyDescriptions() => _result.Content.Should().Contain("User identifier").And.Contain("User name");
}
