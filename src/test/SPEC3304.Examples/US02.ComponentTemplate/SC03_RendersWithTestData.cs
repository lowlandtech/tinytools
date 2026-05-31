namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US02.ComponentTemplate;

/// <summary>
/// Tests template rendering with test data from ComponentTemplate.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "03")]
[UserStory("02", "Component template generates React components")]
public class WhenRenderingComponentTemplateWithTestData : TinyToolsScenario<Shared.Examples.ComponentTemplate>
{
    private TemplateResult? _result;

    protected override Shared.Examples.ComponentTemplate For()
    {
        return new Shared.Examples.ComponentTemplate();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        var data = new ComponentData
        {
            ComponentName = "UserCard",
            Props = new()
            {
                new PropDefinition { Name = "name", Type = "string" },
                new PropDefinition { Name = "age", Type = "number" }
            },
            PropsDestructured = "{ name, age }"
        };
        _result = Sut.Render(data);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Generate Correct Path")]
    [Fact]
    public void ItShouldGenerateCorrectPath()
    {
        ArrangeAndAct();
        _result.Should().NotBeNull();
        _result!.Path.Should().Be("src/components/UserCard.tsx");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Generate Correct Namespace")]
    [Fact]
    public void ItShouldGenerateCorrectNamespace()
    {
        ArrangeAndAct();
        _result!.Namespace.Should().Be("App.Components");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Contain Component Name")]
    [Fact]
    public void ItShouldContainComponentName()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("UserCard");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Contain Props Interface")]
    [Fact]
    public void ItShouldContainPropsInterface()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("export interface UserCardProps");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Contain All Props")]
    [Fact]
    public void ItShouldContainAllProps()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("name: string");
        _result!.Content.Should().Contain("age: number");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Contain Functional Component")]
    [Fact]
    public void ItShouldContainFunctionalComponent()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("export const UserCard: React.FC<UserCardProps>");
    }

    [Trait(Spec.UAC, "07")]
    [Then("it Should Contain Props Destructuring")]
    [Fact]
    public void ItShouldContainPropsDestructuring()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("{ name, age }");
    }

    [Trait(Spec.UAC, "08")]
    [Then("it Should Contain Lowercase Class Name")]
    [Fact]
    public void ItShouldContainLowercaseClassName()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("className=\"usercard\"");
    }
}
