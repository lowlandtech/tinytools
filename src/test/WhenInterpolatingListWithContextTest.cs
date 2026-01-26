namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests for Interpolate extension method for List&lt;string&gt; that takes an ExecutionContext.
/// Note: We use engine: null to disambiguate from the generic Interpolate&lt;T&gt; overload.
/// </summary>
public class WhenInterpolatingListWithContextTest : WhenTestingFor<List<string>>
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

    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("Name", "John");
        _context.Set("City", "Seattle");
        _context.Set("Age", 30);
    }

    protected override void When()
    {
        // Use engine: null to disambiguate overload
        _result = Sut.Interpolate(_context, engine: null);
    }

    [Fact]
    public void ItShouldInterpolateAllTemplates()
    {
        _result.Should().HaveCount(3);
        _result![0].Should().Be("Hello John");
        _result[1].Should().Be("Welcome to Seattle");
        _result[2].Should().Be("Your age is 30");
    }

    [Fact]
    public void ItShouldInterpolateWithCustomEngine()
    {
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

    [Fact]
    public void ItShouldHandleEmptyList()
    {
        // Arrange
        var templates = new List<string>();

        // Act
        var result = templates.Interpolate(_context, engine: null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ItShouldHandleSingleItemList()
    {
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

    [Fact]
    public void ItShouldThrowOnNullTemplates()
    {
        // Arrange
        List<string> templates = null!;

        // Act
        var act = () => templates.Interpolate(_context, engine: null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ItShouldThrowOnNullContext()
    {
        // Arrange
        var templates = new List<string> { "Hello" };
        ExecutionContext context = null!;

        // Act
        var act = () => templates.Interpolate(context, engine: null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ItShouldInterpolateModelFromContext()
    {
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

    [Fact]
    public void ItShouldHandleTemplatesWithNoVariables()
    {
        // Arrange
        var templates = new List<string> { "Hello world!", "Goodbye!" };

        // Act
        var result = templates.Interpolate(_context, engine: null);

        // Assert
        result[0].Should().Be("Hello world!");
        result[1].Should().Be("Goodbye!");
    }

    [Fact]
    public void ItShouldHandleMixedTemplates()
    {
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

    [Fact]
    public void ItShouldSupportIfControlFlowInList()
    {
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

    [Fact]
    public void ItShouldPreserveListOrder()
    {
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

    [Fact]
    public void ItShouldHandleDuplicateTemplates()
    {
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

    [Fact]
    public void ItShouldHandleNestedProperties()
    {
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

    [Fact]
    public void ItShouldSupportNullCoalescingInList()
    {
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
