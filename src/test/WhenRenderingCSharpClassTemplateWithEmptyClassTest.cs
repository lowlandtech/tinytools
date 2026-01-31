using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests template with no properties or methods.
/// </summary>
public class WhenRenderingCSharpClassTemplateWithEmptyClass : WhenTestingFor<CSharpClassTemplate>
{
    private TemplateResult? _result;

    protected override CSharpClassTemplate For()
    {
        return new CSharpClassTemplate();
    }

    protected override void When()
    {
        var data = new ClassData
        {
            Namespace = "Models",
            ClassName = "EmptyClass",
            Description = "An empty class",
            IncludeConstructor = false,
            Properties = new(),
            Methods = new()
        };
        _result = Sut.Render(data);
    }

    [Fact]
    public void ItShouldGenerateBasicClass()
    {
        _result!.Content.Should().Contain("public class EmptyClass");
    }

    [Fact]
    public void ItShouldHaveOnlyClassDeclaration()
    {
        // Should not have property or method declarations (only the class declaration)
        _result!.Content.Should().NotContain("{ get; set; }");
        _result!.Content.Should().NotContain("throw new NotImplementedException");
    }

    [Fact]
    public void ItShouldHaveDescription()
    {
        _result!.Content.Should().Contain("An empty class");
    }
}
