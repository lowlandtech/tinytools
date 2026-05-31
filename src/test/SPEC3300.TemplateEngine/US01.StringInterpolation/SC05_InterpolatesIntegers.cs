namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "05")]
[UserStory("01", "String interpolation handles integer values")]
public class WhenInterpolatingIntegersTest : TinyToolsScenario<string>
{
    private Person _person = null!;
    private string? _result;

    protected override string For()
    {
        return "Hello world, I'm ${Age} years old";
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _person = new Person
        {
            Age = 20
        };
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Interpolate(_person);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Interpolate Age")]
    [Fact]
    public void ItShouldInterpolateAge()
    {
        ArrangeAndAct();
        _result.Should().Be("Hello world, I'm 20 years old");
    }
}