namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US02.ComponentTemplate;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "02")]
[UserStory("02", "Component template generates React components")]
public sealed class SC02_GeneratesWithNoProps : UnitScenario, IClassFixture<UseCases.ComponentTemplateUseCase>
{
    private readonly UseCases.ComponentTemplateUseCase useCase;
    private readonly Shared.Examples.ComponentTemplate _component = new();
    private ComponentData _data = null!;

    public SC02_GeneratesWithNoProps(UseCases.ComponentTemplateUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a component definition with no props")]
    protected override void Given() => _data = new ComponentData
    {
        ComponentName = "EmptyComponent",
        Props = new(),
        PropsDestructured = "{}"
    };

    [When("the component template renders the definition")]
    protected override void When() => useCase.Cases["Result"] = _component.Render(_data);

    [Then("the content should contain export interface EmptyComponentProps")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_GeneratesTheEmptyPropsInterface() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().Contain("export interface EmptyComponentProps");

    [Then("the props interface should be empty")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_LeavesThePropsInterfaceEmpty() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().MatchRegex(@"interface EmptyComponentProps \{\s*\}");

    [Then("the content should contain the empty destructuring")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_UsesEmptyDestructuring() => ((TemplateResult)useCase.Cases["Result"]!).Content.Should().Contain("({})");
}
