using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Real-world scenarios generate practical output")]
public sealed class SC01_GeneratesCode : UnitScenario
{
    private readonly TinyTemplateEngine _engine = new();
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string _result = null!;

    [Given("a code generation template with a customer model")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("ClassName", "Customer");
        _context.Set("Namespace", "MyApp.Models");
        _context.Set("Properties", new List<object>
        {
            new { Name = "Id", Type = "int", IsRequired = true },
            new { Name = "FirstName", Type = "string", IsRequired = true },
            new { Name = "LastName", Type = "string", IsRequired = true },
            new { Name = "Email", Type = "string", IsRequired = false },
            new { Name = "Age", Type = "int?", IsRequired = false }
        });
        _context.Set("HasValidation", true);

        _template = """
            @* Auto-generated code - do not modify *@
            namespace ${Context.Namespace};

            public class ${Context.ClassName}
            {
            @foreach (var prop in Context.Properties) {
                @if (prop.IsRequired) {
                public required ${prop.Type} ${prop.Name} { get; set; }
                } else {
                public ${prop.Type}? ${prop.Name} { get; set; }
                }
            }
            @if (Context.HasValidation) {

                public bool IsValid()
                {
                    return !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName);
                }
            }
            }
            """;
    }

    [When("the engine renders the code generation template")]
    protected override void When() => _result = _engine.Render(_template, _context);

    public SC01_GeneratesCode() => ArrangeAndAct();

    [Then("the content should contain the rendered namespace")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheNamespace() => _result.Should().Contain("namespace MyApp.Models;");

    [Then("the content should contain the rendered class name")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheClassName() => _result.Should().Contain("public class Customer");

    [Then("the content should contain the required properties")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersTheRequiredProperties() => _result.Should().Contain("public required int Id { get; set; }").And.Contain("public required string FirstName { get; set; }");

    [Then("the content should contain the optional property")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersTheOptionalProperties() => _result.Should().Contain("public string? Email { get; set; }");

    [Then("the content should contain the validation method")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_IncludesTheValidationMethod() => _result.Should().Contain("public bool IsValid()");

    [Then("the content should not contain the template comment")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RemovesTheComments() => _result.Should().NotContain("Auto-generated code");
}
