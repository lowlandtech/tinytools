using LowlandTech.TinyTools.Tests.Shared.Examples;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US02.ComponentTemplate;

/// <summary>
/// Tests template rendering with test data from ComponentTemplate.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "03")]
[UserStory("02", "Component template generates React components")]
public class WhenRenderingComponentTemplateWithTestData : WhenTestingFor<Shared.Examples.ComponentTemplate>
{
    private TemplateResult? _result;

    protected override Shared.Examples.ComponentTemplate For()
    {
        return new Shared.Examples.ComponentTemplate();
    }

    protected override void When()
    {
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

    [Fact]
    public void ItShouldGenerateCorrectPath()
    {
        _result.Should().NotBeNull();
        _result!.Path.Should().Be("src/components/UserCard.tsx");
    }

    [Fact]
    public void ItShouldGenerateCorrectNamespace()
    {
        _result!.Namespace.Should().Be("App.Components");
    }

    [Fact]
    public void ItShouldContainComponentName()
    {
        _result!.Content.Should().Contain("UserCard");
    }

    [Fact]
    public void ItShouldContainPropsInterface()
    {
        _result!.Content.Should().Contain("export interface UserCardProps");
    }

    [Fact]
    public void ItShouldContainAllProps()
    {
        _result!.Content.Should().Contain("name: string");
        _result!.Content.Should().Contain("age: number");
    }

    [Fact]
    public void ItShouldContainFunctionalComponent()
    {
        _result!.Content.Should().Contain("export const UserCard: React.FC<UserCardProps>");
    }

    [Fact]
    public void ItShouldContainPropsDestructuring()
    {
        _result!.Content.Should().Contain("{ name, age }");
    }

    [Fact]
    public void ItShouldContainLowercaseClassName()
    {
        _result!.Content.Should().Contain("className=\"usercard\"");
    }
}
