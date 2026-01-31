using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests template rendering with test data from ComponentTemplate.
/// </summary>
public class WhenRenderingComponentTemplateWithTestData : WhenTestingFor<ComponentTemplate>
{
    private TemplateResult? _result;

    protected override ComponentTemplate For()
    {
        return new ComponentTemplate();
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
