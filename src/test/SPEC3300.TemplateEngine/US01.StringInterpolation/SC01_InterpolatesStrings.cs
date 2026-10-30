namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("01", "String interpolation replaces variables from objects")]
public sealed class SC01_InterpolatesStrings : TinyToolsScenario<string>, IClassFixture<UseCases.StringInterpolationUseCase>
{
    private readonly UseCases.StringInterpolationUseCase useCase;
    private Person _person = null!;

    public SC01_InterpolatesStrings(UseCases.StringInterpolationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override string For() => "Hello world, I'm ${FirstName} ${LastName}";

    [Given("a person with a first and last name")]
    protected override void Given() => _person = new Person
    {
        FirstName = "John",
        LastName = "Smith",
    };

    [When("the template is interpolated with the person")]
    protected override void When()
    {
        base.When();
        useCase.Cases["Greeting"] = Sut.Interpolate(_person);
    }

    [Then("the interpolated text should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesFirstAndLastName() => useCase.Cases["Greeting"].Should().Be("Hello world, I'm John Smith");
}
