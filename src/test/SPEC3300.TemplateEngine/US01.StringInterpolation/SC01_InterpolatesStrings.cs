namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("01", "String interpolation replaces variables from objects")]
public sealed class SC01_InterpolatesStrings : TinyToolsScenario<string>
{
    private Person _person = null!;
    private string? _result;

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
        _result = Sut.Interpolate(_person);
    }

    public SC01_InterpolatesStrings() => ArrangeAndAct();

    [Then("the interpolated text should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesFirstAndLastName() => _result.Should().Be("Hello world, I'm John Smith");
}
