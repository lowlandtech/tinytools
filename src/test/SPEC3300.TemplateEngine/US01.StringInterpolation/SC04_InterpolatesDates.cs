namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "04")]
[UserStory("01", "String interpolation handles date values")]
public sealed class SC04_InterpolatesDates : TinyToolsScenario<string>, IClassFixture<UseCases.StringInterpolationUseCase>
{
    private readonly UseCases.StringInterpolationUseCase useCase;
    private Person _person = null!;

    public SC04_InterpolatesDates(UseCases.StringInterpolationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override string For() => "Hello world, date of birth: ${Dob}";

    [Given("a person with a date of birth")]
    protected override void Given() => _person = new Person { Dob = new DateTime(2001, 1, 25) };

    [When("the template is interpolated with the person")]
    protected override void When()
    {
        base.When();
        useCase.Cases["DateOfBirthGreeting"] = Sut.Interpolate(_person);
    }

    [Then("the interpolated text should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesDateOfBirth() => useCase.Cases["DateOfBirthGreeting"].Should().Be("Hello world, date of birth: " + new DateTime(2001, 1, 25));
}
