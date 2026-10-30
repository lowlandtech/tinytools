using System.Text.RegularExpressions;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US01.CSharpClassTemplate;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "02")]
[UserStory("01", "CSharp class template generates class code")]
public sealed class SC02_GeneratesMultipleMembers : UnitScenario
{
    private readonly Shared.Examples.CSharpClassTemplate _template = new();
    private ClassData _data = null!;
    private TemplateResult _result = null!;

    [Given("a service class definition with properties and methods")]
    protected override void Given() => _data = new ClassData
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

    [When("the class template renders the definition")]
    protected override void When() => _result = _template.Render(_data);

    public SC02_GeneratesMultipleMembers() => ArrangeAndAct();

    [Then("the content should contain all properties")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ContainsAllProperties() => _result.Content.Should().Contain("public IUserRepository Repository { get; set; }").And.Contain("public ILogger Logger { get; set; }");

    [Then("the content should contain all methods")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ContainsAllMethods() => _result.Content.Should().Contain("public User GetById(int id)").And.Contain("public void Create(User user)").And.Contain("public bool Update(User user)");

    [Then("the content should contain the method descriptions")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ContainsTheMethodDescriptions() => _result.Content.Should().Contain("Gets user by ID").And.Contain("Creates a new user").And.Contain("Updates existing user");

    [Then("the not implemented bodies should have count 3")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_HasNotImplementedExceptionInAllMethods() => Regex.Matches(_result.Content, @"throw new NotImplementedException\(\);").Count.Should().Be(3);
}
