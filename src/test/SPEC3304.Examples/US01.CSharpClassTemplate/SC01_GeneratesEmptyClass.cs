namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "01")]
[UserStory("01", "CSharp class template generates class code")]
public sealed class SC01_GeneratesEmptyClass : UnitScenario, IClassFixture<UseCases.CSharpClassTemplateUseCase>
{
    private readonly UseCases.CSharpClassTemplateUseCase useCase;
    private readonly Shared.Examples.CSharpClassTemplate _template = new();
    private ClassData _data = null!;

    public SC01_GeneratesEmptyClass(UseCases.CSharpClassTemplateUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("an empty class definition")]
    protected override void Given() => _data = new ClassData
    {
        Namespace = "Models",
        ClassName = "EmptyClass",
        Description = "An empty class",
        IncludeConstructor = false,
        Properties = new(),
        Methods = new()
    };

    [When("the class template renders the definition")]
    protected override void When() => useCase.Cases["Result"] = _template.Render(_data);

    [Then("the content should contain public class EmptyClass")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_GeneratesTheBasicClass() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().Contain("public class EmptyClass");

    [Then("the content should not contain property or method declarations")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HasOnlyTheClassDeclaration() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().NotContain("{ get; set; }").And.NotContain("throw new NotImplementedException");

    [Then("the content should contain An empty class")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasTheDescription() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().Contain("An empty class");
}
