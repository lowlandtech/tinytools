using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

/// <summary>
/// Tests for Interpolate extension method for List&lt;string&gt; that takes an ExecutionContext.
/// Note: We use engine: null to disambiguate from the generic Interpolate&lt;T&gt; overload.
/// </summary>
[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "12")]
[UserStory("01", "String interpolation handles list templates with context")]
public class WhenInterpolatingListWithContextTest : TinyToolsScenario<List<string>>
{
    private ExecutionContext _context = null!;
    private List<string>? _result;

    protected override List<string> For()
    {
        return new List<string>
        {
            "Hello ${Context.Name}",
            "Welcome to ${Context.City}",
            "Your age is ${Context.Age}"
        };
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("Name", "John");
        _context.Set("City", "Seattle");
        _context.Set("Age", 30);
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        // Use engine: null to disambiguate overload
        _result = Sut.Interpolate(_context, engine: null);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Interpolate All Templates")]
    [Fact]
    public void ItShouldInterpolateAllTemplates()
    {
        ArrangeAndAct();
        _result.Should().HaveCount(3);
        _result![0].Should().Be("Hello John");
        _result[1].Should().Be("Welcome to Seattle");
        _result[2].Should().Be("Your age is 30");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Interpolate With Custom Engine")]
    [Fact]
    public void ItShouldInterpolateWithCustomEngine()
    {
        ArrangeAndAct();
        // Arrange
        var templates = new List<string> { "Hello ${Context.Name}", "Bye ${Context.Name}" };
        var context = new ExecutionContext();
        context.Set("Name", "Jane");
        var engine = new TinyTemplateEngine();

        // Act
        var result = templates.Interpolate(context, engine);

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().Be("Hello Jane");
        result[1].Should().Be("Bye Jane");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Handle Empty List")]
    [Fact]
    public void ItShouldHandleEmptyList()
    {
        ArrangeAndAct();
        // Arrange
        var templates = new List<string>();

        // Act
        var result = templates.Interpolate(_context, engine: null);

        // Assert
        result.Should().BeEmpty();
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Handle Single Item List")]
    [Fact]
    public void ItShouldHandleSingleItemList()
    {
        ArrangeAndAct();
        // Arrange
        var templates = new List<string> { "Hello ${Context.Name}" };
        var context = new ExecutionContext();
        context.Set("Name", "John");

        // Act
        var result = templates.Interpolate(context, engine: null);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().Be("Hello John");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Throw On Null Templates")]
    [Fact]
    public void ItShouldThrowOnNullTemplates()
    {
        ArrangeAndAct();
        // Arrange
        List<string> templates = null!;

        // Act
        var act = () => templates.Interpolate(_context, engine: null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Throw On Null Context")]
    [Fact]
    public void ItShouldThrowOnNullContext()
    {
        ArrangeAndAct();
        // Arrange
        var templates = new List<string> { "Hello" };
        ExecutionContext context = null!;

        // Act
        var act = () => templates.Interpolate(context, engine: null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Trait(Spec.UAC, "07")]
    [Then("it Should Interpolate Model From Context")]
    [Fact]
    public void ItShouldInterpolateModelFromContext()
    {
        ArrangeAndAct();
        // Arrange
        var templates = new List<string>
        {
            "${Context.Model.FirstName}",
            "${Context.Model.LastName}"
        };
        var context = new ExecutionContext
        {
            Model = new Person { FirstName = "John", LastName = "Doe" }
        };

        // Act
        var result = templates.Interpolate(context, engine: null);

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().Be("John");
        result[1].Should().Be("Doe");
    }

    [Trait(Spec.UAC, "08")]
    [Then("it Should Handle Templates With No Variables")]
    [Fact]
    public void ItShouldHandleTemplatesWithNoVariables()
    {
        ArrangeAndAct();
        // Arrange
        var templates = new List<string> { "Hello world!", "Goodbye!" };

        // Act
        var result = templates.Interpolate(_context, engine: null);

        // Assert
        result[0].Should().Be("Hello world!");
        result[1].Should().Be("Goodbye!");
    }

    [Trait(Spec.UAC, "09")]
    [Then("it Should Handle Mixed Templates")]
    [Fact]
    public void ItShouldHandleMixedTemplates()
    {
        ArrangeAndAct();
        // Arrange
        var templates = new List<string>
        {
            "Static text",
            "Hello ${Context.Name}",
            "Another static",
            "${Context.City} is great"
        };
        var context = new ExecutionContext();
        context.Set("Name", "John");
        context.Set("City", "Seattle");

        // Act
        var result = templates.Interpolate(context, engine: null);

        // Assert
        result.Should().HaveCount(4);
        result[0].Should().Be("Static text");
        result[1].Should().Be("Hello John");
        result[2].Should().Be("Another static");
        result[3].Should().Be("Seattle is great");
    }

    [Trait(Spec.UAC, "10")]
    [Then("it Should Support If Control Flow In List")]
    [Fact]
    public void ItShouldSupportIfControlFlowInList()
    {
        ArrangeAndAct();
        // Arrange
        var templates = new List<string>
        {
            @"@if (Context.IsActive) {
Active
}"
        };
        var context = new ExecutionContext();
        context.Set("IsActive", true);

        // Act
        var result = templates.Interpolate(context, engine: null);

        // Assert
        result[0].Should().Contain("Active");
    }

    [Trait(Spec.UAC, "11")]
    [Then("it Should Preserve List Order")]
    [Fact]
    public void ItShouldPreserveListOrder()
    {
        ArrangeAndAct();
        // Arrange
        var templates = new List<string>
        {
            "${Context.A}",
            "${Context.B}",
            "${Context.C}",
            "${Context.D}"
        };
        var context = new ExecutionContext();
        context.Set("A", "1");
        context.Set("B", "2");
        context.Set("C", "3");
        context.Set("D", "4");

        // Act
        var result = templates.Interpolate(context, engine: null);

        // Assert
        result.Should().ContainInOrder("1", "2", "3", "4");
    }

    [Trait(Spec.UAC, "12")]
    [Then("it Should Handle Duplicate Templates")]
    [Fact]
    public void ItShouldHandleDuplicateTemplates()
    {
        ArrangeAndAct();
        // Arrange
        var templates = new List<string>
        {
            "Hello ${Context.Name}",
            "Hello ${Context.Name}",
            "Hello ${Context.Name}"
        };
        var context = new ExecutionContext();
        context.Set("Name", "John");

        // Act
        var result = templates.Interpolate(context, engine: null);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllBe("Hello John");
    }

    [Trait(Spec.UAC, "13")]
    [Then("it Should Handle Nested Properties")]
    [Fact]
    public void ItShouldHandleNestedProperties()
    {
        ArrangeAndAct();
        // Arrange
        var templates = new List<string>
        {
            "${Context.User.Name}",
            "${Context.User.Address.City}"
        };
        var context = new ExecutionContext();
        context.Set("User", new { Name = "John", Address = new { City = "Seattle" } });

        // Act
        var result = templates.Interpolate(context, engine: null);

        // Assert
        result[0].Should().Be("John");
        result[1].Should().Be("Seattle");
    }

    [Trait(Spec.UAC, "14")]
    [Then("it Should Support Null Coalescing In List")]
    [Fact]
    public void ItShouldSupportNullCoalescingInList()
    {
        ArrangeAndAct();
        // Arrange
        var templates = new List<string>
        {
            "${Context.Existing ?? \"default1\"}",
            "${Context.Missing ?? \"default2\"}"
        };
        var context = new ExecutionContext();
        context.Set("Existing", "value");

        // Act
        var result = templates.Interpolate(context, engine: null);

        // Assert
        result[0].Should().Be("value");
        result[1].Should().Be("default2");
    }
}
