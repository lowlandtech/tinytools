namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingCodeGenerationTest : WhenTestingFor<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

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

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldRenderNamespace()
    {
        _result.Should().Contain("namespace MyApp.Models;");
    }

    [Fact]
    public void ItShouldRenderClassName()
    {
        _result.Should().Contain("public class Customer");
    }

    [Fact]
    public void ItShouldRenderRequiredProperties()
    {
        _result.Should().Contain("public required int Id { get; set; }");
        _result.Should().Contain("public required string FirstName { get; set; }");
    }

    [Fact]
    public void ItShouldRenderOptionalProperties()
    {
        _result.Should().Contain("public string? Email { get; set; }");
    }

    [Fact]
    public void ItShouldIncludeValidationMethod()
    {
        _result.Should().Contain("public bool IsValid()");
    }

    [Fact]
    public void ItShouldRemoveComments()
    {
        _result.Should().NotContain("Auto-generated code");
    }
}
