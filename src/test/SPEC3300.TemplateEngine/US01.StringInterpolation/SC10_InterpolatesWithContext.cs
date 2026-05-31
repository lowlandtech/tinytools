using ExecutionContext = LowlandTech.TinyTools.Core.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

/// <summary>
/// Tests for Interpolate extension method that takes an ExecutionContext directly.
/// </summary>
[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "10")]
[UserStory("01", "String interpolation uses execution context")]
public class WhenInterpolatingWithContextTest : TinyToolsScenario<string>
{
    private ExecutionContext _context = null!;
    private string? _result;

    protected override string For()
    {
        return "Hello world, I'm ${Context.FirstName} ${Context.LastName}";
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("FirstName", "John");
        _context.Set("LastName", "Smith");
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Interpolate(_context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Interpolate First And Last Name")]
    [Fact]
    public void ItShouldInterpolateFirstAndLastName()
    {
        ArrangeAndAct();
        _result.Should().Be("Hello world, I'm John Smith");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Interpolate With Custom Engine")]
    [Fact]
    public void ItShouldInterpolateWithCustomEngine()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Hello ${Context.Name}";
        var context = new ExecutionContext();
        context.Set("Name", "Jane");
        var engine = new TinyTemplateEngine();

        // Act
        var result = template.Interpolate(context, engine);

        // Assert
        result.Should().Be("Hello Jane");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Interpolate Model From Context")]
    [Fact]
    public void ItShouldInterpolateModelFromContext()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Hello ${Context.Model.FirstName}";
        var context = new ExecutionContext { Model = new Person { FirstName = "John" } };

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Be("Hello John");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Interpolate Nested Context Values")]
    [Fact]
    public void ItShouldInterpolateNestedContextValues()
    {
        ArrangeAndAct();
        // Arrange
        var template = "City: ${Context.Address.City}";
        var context = new ExecutionContext();
        context.Set("Address", new { City = "Seattle", State = "WA" });

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Be("City: Seattle");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Support If Control Flow")]
    [Fact]
    public void ItShouldSupportIfControlFlow()
    {
        ArrangeAndAct();
        // Arrange
        var template = @"@if (Context.IsActive) {
Active User
}";
        var context = new ExecutionContext();
        context.Set("IsActive", true);

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Contain("Active User");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Support Foreach Control Flow")]
    [Fact]
    public void ItShouldSupportForeachControlFlow()
    {
        ArrangeAndAct();
        // Arrange
        var template = @"@foreach (var item in Context.Items) {
- ${item}
}";
        var context = new ExecutionContext();
        context.Set("Items", new[] { "Apple", "Banana", "Cherry" });

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Contain("Apple");
        result.Should().Contain("Banana");
        result.Should().Contain("Cherry");
    }

    [Trait(Spec.UAC, "07")]
    [Then("it Should Throw On Null Template")]
    [Fact]
    public void ItShouldThrowOnNullTemplate()
    {
        ArrangeAndAct();
        // Arrange
        string template = null!;

        // Act
        var act = () => template.Interpolate(_context);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Trait(Spec.UAC, "08")]
    [Then("it Should Throw On Empty Template")]
    [Fact]
    public void ItShouldThrowOnEmptyTemplate()
    {
        ArrangeAndAct();
        // Arrange
        var template = "";

        // Act
        var act = () => template.Interpolate(_context);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Trait(Spec.UAC, "09")]
    [Then("it Should Throw On Null Context")]
    [Fact]
    public void ItShouldThrowOnNullContext()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Hello ${Context.Name}";
        ExecutionContext context = null!;

        // Act
        var act = () => template.Interpolate(context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Trait(Spec.UAC, "10")]
    [Then("it Should Handle Multiple Variables")]
    [Fact]
    public void ItShouldHandleMultipleVariables()
    {
        ArrangeAndAct();
        // Arrange
        var template = "${Context.A} + ${Context.B} = ${Context.C}";
        var context = new ExecutionContext();
        context.Set("A", "1");
        context.Set("B", "2");
        context.Set("C", "3");

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Be("1 + 2 = 3");
    }

    [Trait(Spec.UAC, "11")]
    [Then("it Should Handle Integer Values")]
    [Fact]
    public void ItShouldHandleIntegerValues()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Count: ${Context.Count}";
        var context = new ExecutionContext();
        context.Set("Count", 42);

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Be("Count: 42");
    }

    [Trait(Spec.UAC, "12")]
    [Then("it Should Preserve Non Interpolated Text")]
    [Fact]
    public void ItShouldPreserveNonInterpolatedText()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Hello world! ${Context.Name} says hi.";
        var context = new ExecutionContext();
        context.Set("Name", "John");

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Be("Hello world! John says hi.");
    }

    [Trait(Spec.UAC, "13")]
    [Then("it Should Handle Template With No Variables")]
    [Fact]
    public void ItShouldHandleTemplateWithNoVariables()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Hello world!";

        // Act
        var result = template.Interpolate(_context);

        // Assert
        result.Should().Be("Hello world!");
    }

    [Trait(Spec.UAC, "14")]
    [Then("it Should Support Null Coalescing")]
    [Fact]
    public void ItShouldSupportNullCoalescing()
    {
        ArrangeAndAct();
        // Arrange
        var template = "${Context.Missing ?? \"default\"}";
        var context = new ExecutionContext();

        // Act
        var result = template.Interpolate(context);

        // Assert
        result.Should().Be("default");
    }

    [Trait(Spec.UAC, "15")]
    [Then("it Should Support Child Context")]
    [Fact]
    public void ItShouldSupportChildContext()
    {
        ArrangeAndAct();
        // Arrange
        var template = "${Context.ParentValue} - ${Context.ChildValue}";
        var parentContext = new ExecutionContext();
        parentContext.Set("ParentValue", "Parent");
        var childContext = parentContext.CreateChild();
        childContext.Set("ChildValue", "Child");

        // Act
        var result = template.Interpolate(childContext);

        // Assert
        result.Should().Be("Parent - Child");
    }
}
