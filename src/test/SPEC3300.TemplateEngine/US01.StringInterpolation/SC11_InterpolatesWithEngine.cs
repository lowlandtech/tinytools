namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "11")]
[UserStory("01", "String interpolation uses TinyTemplateEngine")]
public sealed class SC11_InterpolatesWithEngine : TinyToolsScenario<string>
{
    private Person _person = null!;
    private string _name = string.Empty;
    private string _custom = string.Empty;
    private string _integer = string.Empty;
    private string _boolean = string.Empty;
    private string _date = string.Empty;
    private string _conditional = string.Empty;
    private string _ifElse = string.Empty;
    private string _nested = string.Empty;

    protected override string For() => "Hello world, I'm ${Context.Model.FirstName} ${Context.Model.LastName}";

    [Given("a person with a first and last name")]
    protected override void Given() => _person = new Person
    {
        FirstName = "John",
        LastName = "Smith",
    };

    [When("each engine template is interpolated once")]
    protected override void When()
    {
        base.When();

        _name = Sut.InterpolateWithEngine(_person);
        _custom = "Hello ${Context.Model.FirstName}".InterpolateWithEngine(_person, new TinyTemplateEngine());
        _integer = "${Context.Model.FirstName} is ${Context.Model.Age} years old".InterpolateWithEngine(new Person { FirstName = "John", Age = 30 });
        _boolean = "Married: ${Context.Model.IsMarried}".InterpolateWithEngine(new Person { IsMarried = true });
        _date = "Born: ${Context.Model.Dob}".InterpolateWithEngine(new Person { Dob = new DateTime(1990, 5, 15) });
        _conditional = "@if (Context.Model.IsMarried) {\nMarried\n}".InterpolateWithEngine(new Person { IsMarried = true });
        _ifElse = "@if (Context.Model.IsMarried) {\nMarried\n} else {\nSingle\n}".InterpolateWithEngine(new Person { IsMarried = false });
        _nested = "Name: ${Context.Model.Name}, City: ${Context.Model.Address.City}".InterpolateWithEngine(new { Name = "John", Address = new { City = "Seattle" } });
    }

    public SC11_InterpolatesWithEngine() => ArrangeAndAct();

    [Then("the engine output should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesFirstAndLastName() => _name.Should().Be("Hello world, I'm John Smith");

    [Then("the custom engine output should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_InterpolatesWithCustomEngine() => _custom.Should().Be("Hello John");

    [Then("the integer output should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_InterpolatesIntegerProperty() => _integer.Should().Be("John is 30 years old");

    [Then("the boolean output should be exact")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_InterpolatesBooleanProperty() => _boolean.Should().Be("Married: True");

    [Then("the date output should contain 1990")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_InterpolatesDateTimeProperty() => _date.Should().Contain("1990");

    [Then("the conditional output should contain Married")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_SupportsIfControlFlow() => _conditional.Should().Contain("Married");

    [Then("the else branch should contain Single")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_SupportsIfElseControlFlow() => _ifElse.Should().Contain("Single").And.NotContain("Married");

    [Then("the nested property output should be exact")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_HandlesNestedProperties() => _nested.Should().Be("Name: John, City: Seattle");
}
