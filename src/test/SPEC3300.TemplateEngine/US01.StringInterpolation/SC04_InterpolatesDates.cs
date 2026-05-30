namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "04")]
[UserStory("01", "String interpolation handles date values")]
public class WhenInterpolatingDatesTest : TinyToolsScenario<string>
{
    private Person _person = null!;
    private string? _result;

    protected override string For()
    {
        return "Hello world, date of birth: ${Dob}";
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _person = new Person
        {
            Dob = new DateTime(2001, 1, 25)
        };
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Interpolate(_person);
    }

    [Trait(Spec.UAC, "01")]
    [Then("should Interpolate Date Ofb Birth")]
    [Fact]
    public void ShouldInterpolateDateOfbBirth()
    {
        ArrangeAndAct();
        _result.Should().Be("Hello world, date of birth: " + _person.Dob);
    }
}
