namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "09")]
[UserStory("01", "String interpolation validates input parameters")]
public class WhenInterpolatingEmptyStringsTest : TinyToolsScenario<string>
{
    private Person _person = null!;
    private Action? _act;

    protected override string For()
    {
        return "";
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
        _act = () => Sut.Interpolate(_person);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Throw A Template Argument Exception")]
    [Fact]
    public void ItShouldThrowATemplateArgumentException()
    {
        ArrangeAndAct();
        _act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Template should be supplied. (Parameter 'template')");
    }
}
