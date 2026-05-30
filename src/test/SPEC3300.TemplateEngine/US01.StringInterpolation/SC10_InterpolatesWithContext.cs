namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

/// <summary>
/// Tests for Interpolate extension method that takes an ToolContext directly.
/// </summary>
[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "10")]
[UserStory("01", "String interpolation uses execution context")]
public class WhenInterpolatingWithContextTest : WhenTestingFor<string>
{
    private ToolContext _context = null!;
    private string? _result;

    protected override string For()
    {
        return "Hello world, I'm ${Context.FirstName} ${Context.LastName}";
    }

    protected override void Given()
    {
        _context = new ToolContext();
        _context.Set("FirstName", "John");
        _context.Set("LastName", "Smith");
    }

    protected override void When()
    {
        _result = Sut.Interpolate(_context);
    }

    [Fact]
    public void ItShouldInterpolateFirstAndLastName()
    {
        _result.Should().Be("Hello world, I'm John Smith");
    }

    [Fact]
    public void ItShouldInterpolateWithCustomEngine()
    {
        // Arrange
        var template = "Hello ${Context.Name}";
        var context = new ToolContext();
        context.Set("Name", "Jane");
        var engine = new TinyTemplateEngine();

        // Act
        var result = template.Interpolate(context, engine);

        // Assert
        result.Should().Be("Hello Jane");
    }

    [Fact]
    public void ItShouldInterpolateModelFromContext()
    {
        // Arrange
        var template = "Hello ${Context.Model.FirstName}";
        var context = new ToolContext { Model = new Person { FirstName = "John" } };

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Be("Hello John");
    }

    [Fact]
    public void ItShouldInterpolateNestedContextValues()
    {
        // Arrange
        var template = "City: ${Context.Address.City}";
        var context = new ToolContext();
        context.Set("Address", new { City = "Seattle", State = "WA" });

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Be("City: Seattle");
    }

    [Fact]
    public void ItShouldSupportIfControlFlow()
    {
        // Arrange
        var template = @"@if (Context.IsActive) {
Active User
}";
        var context = new ToolContext();
        context.Set("IsActive", true);

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Contain("Active User");
    }

    [Fact]
    public void ItShouldSupportForeachControlFlow()
    {
        // Arrange
        var template = @"@foreach (var item in Context.Items) {
- ${item}
}";
        var context = new ToolContext();
        context.Set("Items", new[] { "Apple", "Banana", "Cherry" });

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Contain("Apple");
        result.Should().Contain("Banana");
        result.Should().Contain("Cherry");
    }

    [Fact]
    public void ItShouldThrowOnNullTemplate()
    {
        // Arrange
        string template = null!;

        // Act
        var act = () => template.Interpolate(_context);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ItShouldThrowOnEmptyTemplate()
    {
        // Arrange
        var template = "";

        // Act
        var act = () => template.Interpolate(_context);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ItShouldThrowOnNullContext()
    {
        // Arrange
        var template = "Hello ${Context.Name}";
        ToolContext context = null!;

        // Act
        var act = () => template.Interpolate(context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ItShouldHandleMultipleVariables()
    {
        // Arrange
        var template = "${Context.A} + ${Context.B} = ${Context.C}";
        var context = new ToolContext();
        context.Set("A", "1");
        context.Set("B", "2");
        context.Set("C", "3");

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Be("1 + 2 = 3");
    }

    [Fact]
    public void ItShouldHandleIntegerValues()
    {
        // Arrange
        var template = "Count: ${Context.Count}";
        var context = new ToolContext();
        context.Set("Count", 42);

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Be("Count: 42");
    }

    [Fact]
    public void ItShouldPreserveNonInterpolatedText()
    {
        // Arrange
        var template = "Hello world! ${Context.Name} says hi.";
        var context = new ToolContext();
        context.Set("Name", "John");

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Be("Hello world! John says hi.");
    }

    [Fact]
    public void ItShouldHandleTemplateWithNoVariables()
    {
        // Arrange
        var template = "Hello world!";

        // Act
        var result = template.Interpolate(_context);

        // Assert
        result.Should().Be("Hello world!");
    }

    [Fact]
    public void ItShouldSupportNullCoalescing()
    {
        // Arrange
        var template = "${Context.Missing ?? \"default\"}";
        var context = new ToolContext();

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Be("default");
    }

    [Fact]
    public void ItShouldSupportChildContext()
    {
        // Arrange
        var template = "${Context.ParentValue} - ${Context.ChildValue}";
        var parentContext = new ToolContext();
        parentContext.Set("ParentValue", "Parent");
        var childContext = parentContext.CreateChild();
        childContext.Set("ChildValue", "Child");

        // Act
        var result = template.Interpolate(childContext);

        // Assert
        result.Should().Be("Parent - Child");
    }
}
