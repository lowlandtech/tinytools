using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "03")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC03_HandlesTernaryAndPropertyAccess : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.StressAndEdgeCasesUseCase>
{
    private readonly UseCases.StressAndEdgeCasesUseCase useCase;
    private readonly ExecutionContext _context09 = new();
    private readonly ExecutionContext _context10 = new();
    private readonly ExecutionContext _context11 = new();
    private readonly ExecutionContext _context12 = new();
    private readonly ExecutionContext _context13 = new();

    public SC03_HandlesTernaryAndPropertyAccess(UseCases.StressAndEdgeCasesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("contexts with a null name, a full name, a status, deep data and user profiles")]
    protected override void Given()
    {
        _context09.Set("Name", null);
        _context10.Set("FirstName", "John");
        _context10.Set("HasLastName", true);
        _context10.Set("LastName", "Doe");
        _context11.Set("Status", "active");
        _context12.Set("Data", new
        {
            Level1 = new { Level2 = new { Level3 = new { Value = "DeepValue" } } }
        });
        _context13.Set("Users", new[]
        {
            new { Profile = new { Name = "Alice", Age = 30 } },
            new { Profile = new { Name = "Bob", Age = 25 } }
        });
    }

    [When("each ternary and property template is rendered once")]
    protected override void When()
    {
        base.When();

        useCase.Cases["GuestGreeting"] = Sut.Render("Hello ${Context.Name != null ? Context.Name : 'Guest'}!", _context09);

        useCase.Cases["FullName"] = Sut.Render("Name: ${Context.HasLastName ? Context.FirstName + ' ' + Context.LastName : Context.FirstName}", _context10);

        useCase.Cases["StatusLabel"] = Sut.Render("Status: ${Context.Status == 'active' ? 'Active' : 'Inactive'}", _context11);

        useCase.Cases["DeepProperty"] = Sut.Render("Value: ${Context.Data.Level1.Level2.Level3.Value}", _context12);

        useCase.Cases["ProfileLines"] = Sut.Render("""
            @foreach (var user in Context.Users) {
            ${user.Profile.Name}: ${user.Profile.Age}
            }
            """, _context13);
    }

    [Then("the guest greeting should be exact")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_HandlesTernaryWithNullValue() => ((string)useCase.Cases["GuestGreeting"]!).Should().Be("Hello Guest!");

    [Then("the full name should be exact")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_HandlesTernaryWithStringConcatenation() => ((string)useCase.Cases["FullName"]!).Should().Be("Name: John Doe");

    [Then("the status label should be exact")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_HandlesNestedTernaryLikePattern() => ((string)useCase.Cases["StatusLabel"]!).Should().Be("Status: Active");

    [Then("the deep property should be exact")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_HandlesDeeplyNestedProperties() => ((string)useCase.Cases["DeepProperty"]!).Should().Be("Value: DeepValue");

    [Then("the render should contain Alice: 30")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_HandlesPropertyAccessOnCollectionItems() => ((string)useCase.Cases["ProfileLines"]!).Should().Contain("Alice: 30").And.Contain("Bob: 25");
}
