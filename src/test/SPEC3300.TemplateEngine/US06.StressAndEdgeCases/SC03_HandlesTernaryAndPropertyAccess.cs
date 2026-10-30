using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US06.StressAndEdgeCases;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "03")]
[UserStory("06", "Template engine handles stress and edge cases")]
public sealed class SC03_HandlesTernaryAndPropertyAccess : TinyToolsScenario<TinyTemplateEngine>
{
    private readonly ExecutionContext _context09 = new();
    private readonly ExecutionContext _context10 = new();
    private readonly ExecutionContext _context11 = new();
    private readonly ExecutionContext _context12 = new();
    private readonly ExecutionContext _context13 = new();
    private string _result09 = string.Empty;
    private string _result10 = string.Empty;
    private string _result11 = string.Empty;
    private string _result12 = string.Empty;
    private string _result13 = string.Empty;

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

        _result09 = Sut.Render("Hello ${Context.Name != null ? Context.Name : 'Guest'}!", _context09);

        _result10 = Sut.Render("Name: ${Context.HasLastName ? Context.FirstName + ' ' + Context.LastName : Context.FirstName}", _context10);

        _result11 = Sut.Render("Status: ${Context.Status == 'active' ? 'Active' : 'Inactive'}", _context11);

        _result12 = Sut.Render("Value: ${Context.Data.Level1.Level2.Level3.Value}", _context12);

        _result13 = Sut.Render("""
            @foreach (var user in Context.Users) {
            ${user.Profile.Name}: ${user.Profile.Age}
            }
            """, _context13);
    }

    public SC03_HandlesTernaryAndPropertyAccess() => ArrangeAndAct();

    [Then("the guest greeting should be exact")]
    [Trait("UAC", "09"), Fact]
    public void UAC09_HandlesTernaryWithNullValue() => _result09.Should().Be("Hello Guest!");

    [Then("the full name should be exact")]
    [Trait("UAC", "10"), Fact]
    public void UAC10_HandlesTernaryWithStringConcatenation() => _result10.Should().Be("Name: John Doe");

    [Then("the status label should be exact")]
    [Trait("UAC", "11"), Fact]
    public void UAC11_HandlesNestedTernaryLikePattern() => _result11.Should().Be("Status: Active");

    [Then("the deep property should be exact")]
    [Trait("UAC", "12"), Fact]
    public void UAC12_HandlesDeeplyNestedProperties() => _result12.Should().Be("Value: DeepValue");

    [Then("the render should contain Alice: 30")]
    [Trait("UAC", "13"), Fact]
    public void UAC13_HandlesPropertyAccessOnCollectionItems() => _result13.Should().Contain("Alice: 30").And.Contain("Bob: 25");
}
