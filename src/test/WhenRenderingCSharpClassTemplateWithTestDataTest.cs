using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Examples;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests template rendering with test data.
/// </summary>
public class WhenRenderingCSharpClassTemplateWithTestData : WhenTestingFor<CSharpClassTemplate>
{
    private TemplateResult? _result;

    protected override CSharpClassTemplate For()
    {
        return new CSharpClassTemplate();
    }

    protected override void When()
    {
        var data = new ClassData
        {
            Namespace = "MyApp.Models",
            ClassName = "User",
            Description = "Represents a user in the system",
            IncludeConstructor = true,
            Properties = new()
            {
                new PropertyData
                {
                    Name = "Id",
                    Type = "int",
                    Description = "User identifier"
                },
                new PropertyData
                {
                    Name = "Name",
                    Type = "string",
                    Description = "User name",
                    DefaultValue = "\"\""
                }
            },
            Methods = new()
            {
                new MethodData
                {
                    Name = "Validate",
                    ReturnType = "bool",
                    Parameters = "",
                    Description = "Validates the user data"
                }
            }
        };
        _result = Sut.Render(data);
    }

    [Fact]
    public void ItShouldGenerateCorrectPath()
    {
        _result.Should().NotBeNull();
        _result!.Path.Should().Be("src/MyApp/Models/User.cs");
    }

    [Fact]
    public void ItShouldGenerateCorrectNamespace()
    {
        _result!.Namespace.Should().Be("MyApp.Models");
    }

    [Fact]
    public void ItShouldContainNamespaceDeclaration()
    {
        _result!.Content.Should().Contain("namespace MyApp.Models;");
    }

    [Fact]
    public void ItShouldContainClassDeclaration()
    {
        _result!.Content.Should().Contain("public class User");
    }

    [Fact]
    public void ItShouldContainClassDescription()
    {
        _result!.Content.Should().Contain("Represents a user in the system");
    }

    [Fact]
    public void ItShouldContainIdProperty()
    {
        _result!.Content.Should().Contain("public int Id { get; set; }");
    }

    [Fact]
    public void ItShouldContainNamePropertyWithDefault()
    {
        _result!.Content.Should().Contain("public string Name { get; set; } = \"\";");
    }

    [Fact]
    public void ItShouldContainConstructor()
    {
        _result!.Content.Should().Contain("public User()");
    }

    [Fact]
    public void ItShouldContainValidateMethod()
    {
        _result!.Content.Should().Contain("public bool Validate()");
        _result!.Content.Should().Contain("throw new NotImplementedException();");
    }

    [Fact]
    public void ItShouldContainPropertyDescriptions()
    {
        _result!.Content.Should().Contain("User identifier");
        _result!.Content.Should().Contain("User name");
    }
}
