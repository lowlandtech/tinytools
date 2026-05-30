namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "06")]
[UserStory("01", "String interpolation replaces variables from dictionaries")]
public class WhenInterpolatingDictionaryTest : TinyToolsScenario<string>
{
    private IDictionary _person = null!;
    private string? _result;

    protected override string For()
    {
        return "Hello world, I'm ${FirstName} ${LastName}";
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _person = new Dictionary<string, string>()
        {
            { "FirstName","John" },
            { "LastName","Smith" }
        };
    }

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Interpolate(_person);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Interpolate First And Last Name")]
    [Fact]
    public void ItShouldInterpolateFirstAndLastName()
    {
        ArrangeAndAct();
        _result.Should().Be("Hello world, I'm John Smith");
    }
}
