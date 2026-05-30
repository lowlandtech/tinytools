namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

/// <summary>
/// Tests template with no properties or methods.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "01")]
[UserStory("01", "CSharp class template generates class code")]
public class WhenRenderingCSharpClassTemplateWithEmptyClass : WhenTestingFor<Shared.Examples.CSharpClassTemplate>
{
    private TemplateResult? _result;

    protected override Shared.Examples.CSharpClassTemplate For()
    {
        return new Shared.Examples.CSharpClassTemplate();
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
