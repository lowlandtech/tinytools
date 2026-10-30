namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Template system validates and renders example templates")]
public sealed class SC01_ValidatesAndRendersExamples : TinyToolsScenario<ComponentTemplate>
{
    private ComponentData _buttonData = null!;
    private ComponentData _profileData = null!;
    private bool _isValid;
    private TemplateResult _button = null!;
    private TemplateResult _profile = null!;

    protected override ComponentTemplate For() => new();

    [Given("the example component template and the data of each case")]
    protected override void Given()
    {
        _buttonData = new ComponentData
        {
            ComponentName = "Button",
            Props = new()
            {
                new PropDefinition { Name = "label", Type = "string" },
                new PropDefinition { Name = "onClick", Type = "() => void" }
            },
            PropsDestructured = "{ label, onClick }"
        };
        _profileData = new ComponentData
        {
            ComponentName = "UserProfile",
            Props = new(),
            PropsDestructured = "{}"
        };
    }

    [When("the template is validated and rendered for each case")]
    protected override void When()
    {
        base.When();
        _isValid = Sut.Validate();
        _button = Sut.Render(_buttonData);
        _profile = Sut.Render(_profileData);
    }

    public SC01_ValidatesAndRendersExamples() => ArrangeAndAct();

    [Then("the component template should be accepted")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ValidatesTheComponentTemplate() => _isValid.Should().BeTrue();

    [Then("the rendered component should contain the ButtonProps interface")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheButtonPropsInterface() => _button.Content.Should().Contain("export interface ButtonProps");

    [Then("the rendered component should contain the label prop")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersTheLabelProp() => _button.Content.Should().Contain("label: string");

    [Then("the rendered component should contain the onClick prop")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersTheOnClickProp() => _button.Content.Should().Contain("onClick: () => void");

    [Then("the rendered component should contain the Button export")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RendersTheButtonExport() => _button.Content.Should().Contain("export const Button");

    [Then("the rendered component path should be src/components/UserProfile.tsx")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RendersTheComponentPath() => _profile.Path.Should().Be("src/components/UserProfile.tsx");
}
