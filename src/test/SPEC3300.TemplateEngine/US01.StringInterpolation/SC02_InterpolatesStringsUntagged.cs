namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "02")]
[UserStory("01", "String interpolation replaces variables without tags")]
public sealed class SC02_InterpolatesStringsUntagged : TinyToolsScenario<string>
{
    private Person _person = null!;
    private string? _result;

    protected override string For() => "Hello world, I'm FirstName LastName";

    [Given("a person with a first and last name")]
    protected override void Given() => _person = new Person
    {
        FirstName = "John",
        LastName = "Smith",
    };

    [When("the template is interpolated without tags")]
    protected override void When()
    {
        base.When();
        _result = Sut.Interpolate(_person, hasTags: false);
    }

    public SC02_InterpolatesStringsUntagged() => ArrangeAndAct();

    [Then("the interpolated text should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesUntaggedNames() => _result.Should().Be("Hello world, I'm John Smith");
}
