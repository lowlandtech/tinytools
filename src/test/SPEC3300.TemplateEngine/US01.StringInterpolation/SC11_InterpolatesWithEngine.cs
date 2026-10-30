namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "11")]
[UserStory("01", "String interpolation uses TinyTemplateEngine")]
public sealed class SC11_InterpolatesWithEngine : TinyToolsScenario<string>, IClassFixture<UseCases.StringInterpolationUseCase>
{
    private readonly UseCases.StringInterpolationUseCase useCase;
    private Person _person = null!;

    public SC11_InterpolatesWithEngine(UseCases.StringInterpolationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

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

        useCase.Cases["Name"] = Sut.InterpolateWithEngine(_person);
        useCase.Cases["CustomEngine"] = "Hello ${Context.Model.FirstName}".InterpolateWithEngine(_person, new TinyTemplateEngine());
        useCase.Cases["Integer"] = "${Context.Model.FirstName} is ${Context.Model.Age} years old".InterpolateWithEngine(new Person { FirstName = "John", Age = 30 });
        useCase.Cases["Boolean"] = "Married: ${Context.Model.IsMarried}".InterpolateWithEngine(new Person { IsMarried = true });
        useCase.Cases["Date"] = "Born: ${Context.Model.Dob}".InterpolateWithEngine(new Person { Dob = new DateTime(1990, 5, 15) });
        useCase.Cases["Conditional"] = "@if (Context.Model.IsMarried) {\nMarried\n}".InterpolateWithEngine(new Person { IsMarried = true });
        useCase.Cases["IfElse"] = "@if (Context.Model.IsMarried) {\nMarried\n} else {\nSingle\n}".InterpolateWithEngine(new Person { IsMarried = false });
        useCase.Cases["Nested"] = "Name: ${Context.Model.Name}, City: ${Context.Model.Address.City}".InterpolateWithEngine(new { Name = "John", Address = new { City = "Seattle" } });
    }

    [Then("the engine output should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesFirstAndLastName() => useCase.Cases["Name"].Should().Be("Hello world, I'm John Smith");

    [Then("the custom engine output should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_InterpolatesWithCustomEngine() => useCase.Cases["CustomEngine"].Should().Be("Hello John");

    [Then("the integer output should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_InterpolatesIntegerProperty() => useCase.Cases["Integer"].Should().Be("John is 30 years old");

    [Then("the boolean output should be exact")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_InterpolatesBooleanProperty() => useCase.Cases["Boolean"].Should().Be("Married: True");

    [Then("the date output should contain 1990")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_InterpolatesDateTimeProperty() => ((string)useCase.Cases["Date"]!).Should().Contain("1990");

    [Then("the conditional output should contain Married")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_SupportsIfControlFlow() => ((string)useCase.Cases["Conditional"]!).Should().Contain("Married");

    [Then("the else branch should contain Single")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_SupportsIfElseControlFlow() => ((string)useCase.Cases["IfElse"]!).Should().Contain("Single").And.NotContain("Married");

    [Then("the nested property output should be exact")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_HandlesNestedProperties() => useCase.Cases["Nested"].Should().Be("Name: John, City: Seattle");
}
