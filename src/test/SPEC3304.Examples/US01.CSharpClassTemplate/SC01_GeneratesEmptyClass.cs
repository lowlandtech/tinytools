namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

/// <summary>
/// Tests template with no properties or methods.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "01")]
[UserStory("01", "CSharp class template generates class code")]
public class WhenRenderingCSharpClassTemplateWithEmptyClass : TinyToolsScenario<Shared.Examples.CSharpClassTemplate>
{
    private TemplateResult? _result;

    protected override Shared.Examples.CSharpClassTemplate For()
    {
        return new Shared.Examples.CSharpClassTemplate();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
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

    [Trait(Spec.UAC, "01")]
    [Then("it Should Generate Basic Class")]
    [Fact]
    public void ItShouldGenerateBasicClass()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("public class EmptyClass");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Have Only Class Declaration")]
    [Fact]
    public void ItShouldHaveOnlyClassDeclaration()
    {
        ArrangeAndAct();
        // Should not have property or method declarations (only the class declaration)
        _result!.Content.Should().NotContain("{ get; set; }");
        _result!.Content.Should().NotContain("throw new NotImplementedException");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Have Description")]
    [Fact]
    public void ItShouldHaveDescription()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("An empty class");
    }
}
