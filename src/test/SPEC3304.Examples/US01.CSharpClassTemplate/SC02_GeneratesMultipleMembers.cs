namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

/// <summary>
/// Tests template with multiple methods and properties.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "02")]
[UserStory("01", "CSharp class template generates class code")]
public class WhenRenderingCSharpClassTemplateWithMultipleMembers : TinyToolsScenario<Shared.Examples.CSharpClassTemplate>
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
            Namespace = "Services",
            ClassName = "UserService",
            Description = "Service for managing users",
            IncludeConstructor = true,
            Properties = new()
            {
                new PropertyData { Name = "Repository", Type = "IUserRepository", Description = "User repository" },
                new PropertyData { Name = "Logger", Type = "ILogger", Description = "Logger instance" }
            },
            Methods = new()
            {
                new MethodData { Name = "GetById", ReturnType = "User", Parameters = "int id", Description = "Gets user by ID" },
                new MethodData { Name = "Create", ReturnType = "void", Parameters = "User user", Description = "Creates a new user" },
                new MethodData { Name = "Update", ReturnType = "bool", Parameters = "User user", Description = "Updates existing user" }
            }
        };
        _result = Sut.Render(data);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Contain All Properties")]
    [Fact]
    public void ItShouldContainAllProperties()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("public IUserRepository Repository { get; set; }");
        _result!.Content.Should().Contain("public ILogger Logger { get; set; }");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Contain All Methods")]
    [Fact]
    public void ItShouldContainAllMethods()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("public User GetById(int id)");
        _result!.Content.Should().Contain("public void Create(User user)");
        _result!.Content.Should().Contain("public bool Update(User user)");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Contain Method Descriptions")]
    [Fact]
    public void ItShouldContainMethodDescriptions()
    {
        ArrangeAndAct();
        _result!.Content.Should().Contain("Gets user by ID");
        _result!.Content.Should().Contain("Creates a new user");
        _result!.Content.Should().Contain("Updates existing user");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Have Not Implemented Exception In All Methods")]
    [Fact]
    public void ItShouldHaveNotImplementedExceptionInAllMethods()
    {
        ArrangeAndAct();
        var notImplementedCount = System.Text.RegularExpressions.Regex.Matches(
            _result!.Content, 
            "throw new NotImplementedException\\(\\);").Count;
        
        notImplementedCount.Should().Be(3);
    }
}
