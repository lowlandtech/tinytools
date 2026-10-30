namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US02.ComponentTemplate;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "03")]
[UserStory("02", "Component template generates React components")]
public sealed class SC03_RendersWithTestData : UnitScenario
{
    private readonly Shared.Examples.ComponentTemplate _component = new();
    private ComponentData _data = null!;
    private TemplateResult _result = null!;

    [Given("the shared example component data")]
    protected override void Given() => _data = new ComponentData
    {
        ComponentName = "UserCard",
        Props = new()
        {
            new PropDefinition { Name = "name", Type = "string" },
            new PropDefinition { Name = "age", Type = "number" }
        },
        PropsDestructured = "{ name, age }"
    };

    [When("the component template renders the definition")]
    protected override void When() => _result = _component.Render(_data);

    public SC03_RendersWithTestData() => ArrangeAndAct();

    [Then("the result should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ReturnsAResult() => _result.Should().NotBeNull();

    [Then("the generated path should be src/components/UserCard.tsx")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_GeneratesTheCorrectPath() => _result.Path.Should().Be("src/components/UserCard.tsx");

    [Then("the namespace should be App.Components")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_GeneratesTheCorrectNamespace() => _result.Namespace.Should().Be("App.Components");

    [Then("the content should contain UserCard")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ContainsTheComponentName() => _result.Content.Should().Contain("UserCard");

    [Then("the content should contain the props interface")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ContainsThePropsInterface() => _result.Content.Should().Contain("export interface UserCardProps");

    [Then("the content should contain all props")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ContainsAllProps() => _result.Content.Should().Contain("name: string").And.Contain("age: number");

    [Then("the content should contain the functional component declaration")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ContainsTheFunctionalComponent() => _result.Content.Should().Contain("export const UserCard: React.FC<UserCardProps>");

    [Then("the content should contain the props destructuring")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_ContainsThePropsDestructuring() => _result.Content.Should().Contain("{ name, age }");

    [Then("the content should contain the lowercase class name")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_ContainsTheLowercaseClassName() => _result.Content.Should().Contain("className=\"usercard\"");
}
