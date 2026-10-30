namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "01")]
[UserStory("01", "CSharp class template generates class code")]
public sealed class SC01_GeneratesEmptyClass : UnitScenario
{
    private readonly Shared.Examples.CSharpClassTemplate _template = new();
    private ClassData _data = null!;
    private TemplateResult _result = null!;

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
    protected override void When() => _result = _template.Render(_data);

    public SC01_GeneratesEmptyClass() => ArrangeAndAct();

    [Then("the content should contain public class EmptyClass")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_GeneratesTheBasicClass() => _result.Content.Should().Contain("public class EmptyClass");

    [Then("the content should not contain property or method declarations")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_HasOnlyTheClassDeclaration() => _result.Content.Should().NotContain("{ get; set; }").And.NotContain("throw new NotImplementedException");

    [Then("the content should contain An empty class")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_HasTheDescription() => _result.Content.Should().Contain("An empty class");
}
