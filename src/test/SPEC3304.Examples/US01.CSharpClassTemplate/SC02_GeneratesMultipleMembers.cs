namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

/// <summary>
/// Tests template with multiple methods and properties.
/// </summary>
[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "02")]
[UserStory("01", "CSharp class template generates class code")]
public class WhenRenderingCSharpClassTemplateWithMultipleMembers : WhenTestingFor<Shared.Examples.CSharpClassTemplate>
{
    private TemplateResult? _result;

    protected override Shared.Examples.CSharpClassTemplate For()
    {
        return new Shared.Examples.CSharpClassTemplate();
    }

    protected override void When()
    {
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

    [Fact]
    public void ItShouldContainAllProperties()
    {
        _result!.Content.Should().Contain("public IUserRepository Repository { get; set; }");
        _result!.Content.Should().Contain("public ILogger Logger { get; set; }");
    }

    [Fact]
    public void ItShouldContainAllMethods()
    {
        _result!.Content.Should().Contain("public User GetById(int id)");
        _result!.Content.Should().Contain("public void Create(User user)");
        _result!.Content.Should().Contain("public bool Update(User user)");
    }

    [Fact]
    public void ItShouldContainMethodDescriptions()
    {
        _result!.Content.Should().Contain("Gets user by ID");
        _result!.Content.Should().Contain("Creates a new user");
        _result!.Content.Should().Contain("Updates existing user");
    }

    [Fact]
    public void ItShouldHaveNotImplementedExceptionInAllMethods()
    {
        var notImplementedCount = System.Text.RegularExpressions.Regex.Matches(
            _result!.Content, 
            "throw new NotImplementedException\\(\\);").Count;
        
        notImplementedCount.Should().Be(3);
    }
}
