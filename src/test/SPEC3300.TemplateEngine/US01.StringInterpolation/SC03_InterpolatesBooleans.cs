namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "03")]
[UserStory("01", "String interpolation handles boolean values")]
public class WhenInterpolatingBooleansTest : TinyToolsScenario<string>
{
    private Person _person = null!;
    private string? _result;

    protected override string For()
    {
        return "Hello world, married: ${IsMarried}";
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _person = new Person
        {
            IsMarried = true
        };
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Interpolate(_person);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Interpolate Is Married")]
    [Fact]
    public void ItShouldInterpolateIsMarried()
    {
        ArrangeAndAct();
        _result.Should().Be("Hello world, married: True");
    }
}
