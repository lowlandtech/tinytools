namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "03")]
[UserStory("01", "String interpolation handles boolean values")]
public sealed class SC03_InterpolatesBooleans : TinyToolsScenario<string>, IClassFixture<UseCases.StringInterpolationUseCase>
{
    private readonly UseCases.StringInterpolationUseCase useCase;
    private Person _person = null!;

    public SC03_InterpolatesBooleans(UseCases.StringInterpolationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override string For() => "Hello world, married: ${IsMarried}";

    [Given("a person who is married")]
    protected override void Given() => _person = new Person { IsMarried = true };

    [When("the template is interpolated with the person")]
    protected override void When()
    {
        base.When();
        useCase.Cases["MarriedGreeting"] = Sut.Interpolate(_person);
    }

    [Then("the interpolated text should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesIsMarried() => useCase.Cases["MarriedGreeting"].Should().Be("Hello world, married: True");
}
