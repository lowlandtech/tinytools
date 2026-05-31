namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "08")]
[UserStory("01", "String interpolation validates input parameters")]
public class WhenInterpolatingNullModelTest : TinyToolsScenario<string>
{
    private Person? _person;
    private Action? _act;

    protected override string For()
    {
        return "Hello world, I'm {FirstName} {LastName}";

    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _person = null;
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _act = () => Sut.Interpolate(_person);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Throw A Model Argument Exception")]
    [Fact]
    public void ItShouldThrowAModelArgumentException()
    {
        ArrangeAndAct();
        _act.Should()
            .Throw<ArgumentException>();
    }
}
