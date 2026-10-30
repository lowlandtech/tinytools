namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US02.ComponentTemplate;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "02")]
[UserStory("02", "Component template generates React components")]
public sealed class SC02_GeneratesWithNoProps : UnitScenario
{
    private readonly Shared.Examples.ComponentTemplate _component = new();
    private ComponentData _data = null!;
    private TemplateResult _result = null!;

    [Given("a component definition with no props")]
    protected override void Given() => _data = new ComponentData
    {
        ComponentName = "EmptyComponent",
        Props = new(),
        PropsDestructured = "{}"
    };

    [When("the component template renders the definition")]
    protected override void When() => _result = _component.Render(_data);

    public SC02_GeneratesWithNoProps() => ArrangeAndAct();

    [Then("the content should contain export interface EmptyComponentProps")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_GeneratesTheEmptyPropsInterface() => _result.Content.Should().Contain("export interface EmptyComponentProps");

    [Then("the props interface should be empty")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_LeavesThePropsInterfaceEmpty() => _result.Content.Should().MatchRegex(@"interface EmptyComponentProps \{\s*\}");

    [Then("the content should contain the empty destructuring")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_UsesEmptyDestructuring() => _result.Content.Should().Contain("({})");
}
