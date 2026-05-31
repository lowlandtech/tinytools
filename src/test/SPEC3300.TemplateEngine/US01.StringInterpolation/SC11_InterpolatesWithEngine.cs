namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

/// <summary>
/// Tests for InterpolateWithEngine extension method that uses the TinyTemplateEngine
/// with ${Context.Model.xxx} syntax.
/// </summary>
[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "11")]
[UserStory("01", "String interpolation uses TinyTemplateEngine")]
public class WhenInterpolatingWithEngineTest : TinyToolsScenario<string>
{
    private Person _person = null!;
    private string? _result;

    protected override string For()
    {
        return "Hello world, I'm ${Context.Model.FirstName} ${Context.Model.LastName}";
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _person = new Person
        {
            FirstName = "John",
            LastName = "Smith"
        };
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.InterpolateWithEngine(_person);
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
        var template = "Hello ${Context.Model.FirstName}";
        var engine = new TinyTemplateEngine();

        // Act
        var result = template.InterpolateWithEngine(_person, engine);

        // Assert
        result.Should().Be("Hello John");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Interpolate Integer Property")]
    [Fact]
    public void ItShouldInterpolateIntegerProperty()
    {
        ArrangeAndAct();
        // Arrange
        var template = "${Context.Model.FirstName} is ${Context.Model.Age} years old";
        var person = new Person { FirstName = "John", Age = 30 };

        // Act
        var result = template.InterpolateWithEngine(person);

        // Assert
        result.Should().Be("John is 30 years old");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Interpolate Boolean Property")]
    [Fact]
    public void ItShouldInterpolateBooleanProperty()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Married: ${Context.Model.IsMarried}";
        var person = new Person { IsMarried = true };

        // Act
        var result = template.InterpolateWithEngine(person);

        // Assert
        result.Should().Be("Married: True");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Interpolate Date Time Property")]
    [Fact]
    public void ItShouldInterpolateDateTimeProperty()
    {
        ArrangeAndAct();
        // Arrange
        var dob = new DateTime(1990, 5, 15);
        var template = "Born: ${Context.Model.Dob}";
        var person = new Person { Dob = dob };

        // Act
        var result = template.InterpolateWithEngine(person);

        // Assert
        result.Should().Contain("1990");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Support If Control Flow")]
    [Fact]
    public void ItShouldSupportIfControlFlow()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "07")]
    [Then("it Should Support If Else Control Flow")]
    [Fact]
    public void ItShouldSupportIfElseControlFlow()
    {
        ArrangeAndAct();
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

    [Trait(Spec.UAC, "08")]
    [Then("it Should Handle Nested Properties")]
    [Fact]
    public void ItShouldHandleNestedProperties()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Name: ${Context.Model.Name}, City: ${Context.Model.Address.City}";
        var model = new { Name = "John", Address = new { City = "Seattle" } };

        // Act
        var result = template.InterpolateWithEngine(model);

        // Assert
        result.Should().Be("Name: John, City: Seattle");
    }

    [Trait(Spec.UAC, "09")]
    [Then("it Should Handle Anonymous Types")]
    [Fact]
    public void ItShouldHandleAnonymousTypes()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Hello ${Context.Model.Name}, you have ${Context.Model.Count} items";
        var model = new { Name = "John", Count = 5 };

        // Act
        var result = template.InterpolateWithEngine(model);

        // Assert
        result.Should().Be("Hello John, you have 5 items");
    }

    [Trait(Spec.UAC, "10")]
    [Then("it Should Throw On Null Template")]
    [Fact]
    public void ItShouldThrowOnNullTemplate()
    {
        ArrangeAndAct();
        // Arrange
        string template = null!;

        // Act
        var act = () => template.InterpolateWithEngine(_person);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Trait(Spec.UAC, "11")]
    [Then("it Should Throw On Empty Template")]
    [Fact]
    public void ItShouldThrowOnEmptyTemplate()
    {
        ArrangeAndAct();
        // Arrange
        var template = "";

        // Act
        var act = () => template.InterpolateWithEngine(_person);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Trait(Spec.UAC, "12")]
    [Then("it Should Throw On Null Model")]
    [Fact]
    public void ItShouldThrowOnNullModel()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Hello ${Context.Model.Name}";
        Person model = null!;

        // Act
        var act = () => template.InterpolateWithEngine(model);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Trait(Spec.UAC, "13")]
    [Then("it Should Handle Dictionary Model")]
    [Fact]
    public void ItShouldHandleDictionaryModel()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Hello ${Context.Model.Name}";
        var model = new Dictionary<string, object> { { "Name", "John" } };

        // Act
        var result = template.InterpolateWithEngine(model);

        // Assert
        result.Should().Be("Hello John");
    }

    [Trait(Spec.UAC, "14")]
    [Then("it Should Handle Multiple Interpolations")]
    [Fact]
    public void ItShouldHandleMultipleInterpolations()
    {
        ArrangeAndAct();
        // Arrange
        var template = "${Context.Model.FirstName} ${Context.Model.FirstName} ${Context.Model.LastName}";
        var person = new Person { FirstName = "John", LastName = "Doe" };

        // Act
        var result = template.InterpolateWithEngine(person);

        // Assert
        result.Should().Be("John John Doe");
    }

    [Trait(Spec.UAC, "15")]
    [Then("it Should Preserve Non Interpolated Text")]
    [Fact]
    public void ItShouldPreserveNonInterpolatedText()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Hello world! ${Context.Model.FirstName} says hi.";
        var person = new Person { FirstName = "John" };

        // Act
        var result = template.InterpolateWithEngine(person);

        // Assert
        result.Should().Be("Hello world! John says hi.");
    }

    [Trait(Spec.UAC, "16")]
    [Then("it Should Handle Template With No Variables")]
    [Fact]
    public void ItShouldHandleTemplateWithNoVariables()
    {
        ArrangeAndAct();
        // Arrange
        var template = "Hello world!";

        // Act
        var result = template.InterpolateWithEngine(_person);

        // Assert
        result.Should().Be("Hello world!");
    }
}
