namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Real-world scenarios generate practical output")]
public class WhenRenderingCodeGenerationTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ToolContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _context = new ToolContext();
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Render Namespace")]
    [Fact]
    public void ItShouldRenderNamespace()
    {
        ArrangeAndAct();
        _result.Should().Contain("namespace MyApp.Models;");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Render Class Name")]
    [Fact]
    public void ItShouldRenderClassName()
    {
        ArrangeAndAct();
        _result.Should().Contain("public class Customer");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Render Required Properties")]
    [Fact]
    public void ItShouldRenderRequiredProperties()
    {
        ArrangeAndAct();
        _result.Should().Contain("public required int Id { get; set; }");
        _result.Should().Contain("public required string FirstName { get; set; }");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Render Optional Properties")]
    [Fact]
    public void ItShouldRenderOptionalProperties()
    {
        ArrangeAndAct();
        _result.Should().Contain("public string? Email { get; set; }");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Include Validation Method")]
    [Fact]
    public void ItShouldIncludeValidationMethod()
    {
        ArrangeAndAct();
        _result.Should().Contain("public bool IsValid()");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Remove Comments")]
    [Fact]
    public void ItShouldRemoveComments()
    {
        ArrangeAndAct();
        _result.Should().NotContain("Auto-generated code");
    }
}
