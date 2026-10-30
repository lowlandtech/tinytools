namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "09")]
[UserStory("01", "String interpolation validates input parameters")]
public sealed class SC09_ThrowsOnEmptyTemplate : TinyToolsScenario<string>, IClassFixture<UseCases.StringInterpolationUseCase>
{
    private readonly UseCases.StringInterpolationUseCase useCase;
    private Person _person = null!;

    public SC09_ThrowsOnEmptyTemplate(UseCases.StringInterpolationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override string For() => "";

    [Given("a person with a first and last name")]
    protected override void Given() => _person = new Person
    {
        FirstName = "John",
        LastName = "Smith",
    };

    [When("the empty template is interpolated")]
    protected override void When()
    {
        base.When();
        useCase.Cases["EmptyTemplate"] = (Action)(() => Sut.Interpolate(_person));
    }

    [Then("the interpolation should fail with ArgumentException")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ThrowsOnEmptyTemplate() => ((Action)useCase.Cases["EmptyTemplate"]!).Should().Throw<ArgumentException>().WithMessage("Template should be supplied. (Parameter 'template')");
}
