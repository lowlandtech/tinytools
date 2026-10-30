namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US02.ComponentTemplate;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Component template generates React components")]
public sealed class SC01_GeneratesWithCustomData : UnitScenario
{
    private readonly Shared.Examples.ComponentTemplate _component = new();
    private ComponentData _data = null!;
    private TemplateResult _result = null!;

    [Given("a button component definition with three props")]
    protected override void Given() => _data = new ComponentData
    {
        ComponentName = "Button",
        Props = new()
        {
            new PropDefinition { Name = "label", Type = "string" },
            new PropDefinition { Name = "onClick", Type = "() => void" },
            new PropDefinition { Name = "disabled", Type = "boolean" }
        },
        PropsDestructured = "{ label, onClick, disabled }"
    };

    [When("the component template renders the definition")]
    protected override void When() => _result = _component.Render(_data);

    public SC01_GeneratesWithCustomData() => ArrangeAndAct();

    [Then("the generated path should be src/components/Button.tsx")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_GeneratesTheButtonPath() => _result.Path.Should().Be("src/components/Button.tsx");

    [Then("the content should contain export interface ButtonProps")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ContainsTheButtonInterface() => _result.Content.Should().Contain("export interface ButtonProps");

    [Then("the content should contain all three prop types")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ContainsAllThreeProps() => _result.Content.Should().Contain("label: string").And.Contain("onClick: () => void").And.Contain("disabled: boolean");

    [Then("the content should contain export const Button")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ContainsTheButtonComponent() => _result.Content.Should().Contain("export const Button");

    [Then("the content should contain the lowercase button class name")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_UsesTheLowercaseClassName() => _result.Content.Should().Contain("className=\"button\"");
}
