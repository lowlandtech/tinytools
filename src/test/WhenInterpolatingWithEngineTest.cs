namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests for InterpolateWithEngine extension method that uses the TinyTemplateEngine
/// with ${Context.Model.xxx} syntax.
/// </summary>
public class WhenInterpolatingWithEngineTest : WhenTestingFor<string>
{
    private Person _person = null!;
    private string? _result;

    protected override string For()
    {
        return "Hello world, I'm ${Context.Model.FirstName} ${Context.Model.LastName}";
    }

    protected override void Given()
    {
        _person = new Person
        {
            FirstName = "John",
            LastName = "Smith"
        };
    }

    protected override void When()
    {
        _result = Sut.InterpolateWithEngine(_person);
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
        var template = "Hello ${Context.Model.FirstName}";
        var engine = new TinyTemplateEngine();

        // Act
        var result = template.InterpolateWithEngine(_person, engine);

        // Assert
        result.Should().Be("Hello John");
    }

    [Fact]
    public void ItShouldInterpolateIntegerProperty()
    {
        // Arrange
        var template = "${Context.Model.FirstName} is ${Context.Model.Age} years old";
        var person = new Person { FirstName = "John", Age = 30 };

        // Act
        var result = template.InterpolateWithEngine(person);

        // Assert
        result.Should().Be("John is 30 years old");
    }

    [Fact]
    public void ItShouldInterpolateBooleanProperty()
    {
        // Arrange
        var template = "Married: ${Context.Model.IsMarried}";
        var person = new Person { IsMarried = true };

        // Act
        var result = template.InterpolateWithEngine(person);

        // Assert
        result.Should().Be("Married: True");
    }

    [Fact]
    public void ItShouldInterpolateDateTimeProperty()
    {
        // Arrange
        var dob = new DateTime(1990, 5, 15);
        var template = "Born: ${Context.Model.Dob}";
        var person = new Person { Dob = dob };

        // Act
        var result = template.InterpolateWithEngine(person);

        // Assert
        result.Should().Contain("1990");
    }

    [Fact]
    public void ItShouldSupportIfControlFlow()
    {
        // Arrange
        var template = @"@if (Context.Model.IsMarried) {
Married
}";
        var person = new Person { IsMarried = true };

        // Act
        var result = template.InterpolateWithEngine(person);

        // Assert
        result.Should().Contain("Married");
    }

    [Fact]
    public void ItShouldSupportIfElseControlFlow()
    {
        // Arrange
        var template = @"@if (Context.Model.IsMarried) {
Married
} else {
Single
}";
        var person = new Person { IsMarried = false };

        // Act
        var result = template.InterpolateWithEngine(person);

        // Assert
        result.Should().Contain("Single");
        result.Should().NotContain("Married");
    }

    [Fact]
    public void ItShouldHandleNestedProperties()
    {
        // Arrange
        var template = "Name: ${Context.Model.Name}, City: ${Context.Model.Address.City}";
        var model = new { Name = "John", Address = new { City = "Seattle" } };

        // Act
        var result = template.InterpolateWithEngine(model);

        // Assert
        result.Should().Be("Name: John, City: Seattle");
    }

    [Fact]
    public void ItShouldHandleAnonymousTypes()
    {
        // Arrange
        var template = "Hello ${Context.Model.Name}, you have ${Context.Model.Count} items";
        var model = new { Name = "John", Count = 5 };

        // Act
        var result = template.InterpolateWithEngine(model);

        // Assert
        result.Should().Be("Hello John, you have 5 items");
    }

    [Fact]
    public void ItShouldThrowOnNullTemplate()
    {
        // Arrange
        string template = null!;

        // Act
        var act = () => template.InterpolateWithEngine(_person);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ItShouldThrowOnEmptyTemplate()
    {
        // Arrange
        var template = "";

        // Act
        var act = () => template.InterpolateWithEngine(_person);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ItShouldThrowOnNullModel()
    {
        // Arrange
        var template = "Hello ${Context.Model.Name}";
        Person model = null!;

        // Act
        var act = () => template.InterpolateWithEngine(model);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ItShouldHandleDictionaryModel()
    {
        // Arrange
        var template = "Hello ${Context.Model.Name}";
        var model = new Dictionary<string, object> { { "Name", "John" } };

        // Act
        var result = template.InterpolateWithEngine(model);

        // Assert
        result.Should().Be("Hello John");
    }

    [Fact]
    public void ItShouldHandleMultipleInterpolations()
    {
        // Arrange
        var template = "${Context.Model.FirstName} ${Context.Model.FirstName} ${Context.Model.LastName}";
        var person = new Person { FirstName = "John", LastName = "Doe" };

        // Act
        var result = template.InterpolateWithEngine(person);

        // Assert
        result.Should().Be("John John Doe");
    }

    [Fact]
    public void ItShouldPreserveNonInterpolatedText()
    {
        // Arrange
        var template = "Hello world! ${Context.Model.FirstName} says hi.";
        var person = new Person { FirstName = "John" };

        // Act
        var result = template.InterpolateWithEngine(person);

        // Assert
        result.Should().Be("Hello world! John says hi.");
    }

    [Fact]
    public void ItShouldHandleTemplateWithNoVariables()
    {
        // Arrange
        var template = "Hello world!";

        // Act
        var result = template.InterpolateWithEngine(_person);

        // Assert
        result.Should().Be("Hello world!");
    }
}
