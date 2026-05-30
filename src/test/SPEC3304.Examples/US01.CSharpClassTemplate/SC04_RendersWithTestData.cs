namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

/// <summary>
/// Tests template rendering with test data.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "04")]
[UserStory("01", "CSharp class template generates class code")]
public class WhenRenderingCSharpClassTemplateWithTestData : TinyToolsScenario<Shared.Examples.CSharpClassTemplate>
{
    private TemplateResult? _result;

    protected override Shared.Examples.CSharpClassTemplate For()
    {
        return new Shared.Examples.CSharpClassTemplate();
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
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

    [Trait(Spec.UAC, "01")]
    [Then("it Should Generate Correct Path")]
    [Fact]
    public void ItShouldGenerateCorrectPath()
    {
        ArrangeAndAct();
        _result.Should().NotBeNull();
        _result!.Path.Should().Be("src/MyApp/Models/User.cs");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Generate Correct Namespace")]
    [Fact]
    public void ItShouldGenerateCorrectNamespace()
    {
        ArrangeAndAct();
        _result!.Namespace.Should().Be("MyApp.Models");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Contain Namespace Declaration")]
    [Fact]
    public void ItShouldContainNamespaceDeclaration()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("namespace MyApp.Models;");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Contain Class Declaration")]
    [Fact]
    public void ItShouldContainClassDeclaration()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("public class User");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Contain Class Description")]
    [Fact]
    public void ItShouldContainClassDescription()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("Represents a user in the system");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Contain Id Property")]
    [Fact]
    public void ItShouldContainIdProperty()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("public int Id { get; set; }");
    }

    [Trait(Spec.UAC, "07")]
    [Then("it Should Contain Name Property With Default")]
    [Fact]
    public void ItShouldContainNamePropertyWithDefault()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("public string Name { get; set; } = \"\";");
    }

    [Trait(Spec.UAC, "08")]
    [Then("it Should Contain Constructor")]
    [Fact]
    public void ItShouldContainConstructor()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("public User()");
    }

    [Trait(Spec.UAC, "09")]
    [Then("it Should Contain Validate Method")]
    [Fact]
    public void ItShouldContainValidateMethod()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("public bool Validate()");
        _result!.Content.Should().Contain("throw new NotImplementedException();");
    }

    [Trait(Spec.UAC, "10")]
    [Then("it Should Contain Property Descriptions")]
    [Fact]
    public void ItShouldContainPropertyDescriptions()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("User identifier");
        _result!.Content.Should().Contain("User name");
    }
}
