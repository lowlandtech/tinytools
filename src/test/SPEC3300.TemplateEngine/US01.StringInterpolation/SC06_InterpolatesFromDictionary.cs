namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "06")]
[UserStory("01", "String interpolation replaces variables from dictionaries")]
public sealed class SC06_InterpolatesFromDictionary : TinyToolsScenario<string>
{
    private IDictionary _person = null!;
    private string? _result;

    protected override string For() => "Hello world, I'm ${FirstName} ${LastName}";

    [Given("a dictionary holding a first and last name")]
    protected override void Given() => _person = new Dictionary<string, string>
    {
        { "FirstName", "John" },
        { "LastName", "Smith" },
    };

    [When("the template is interpolated with the dictionary")]
    protected override void When()
    {
        base.When();
        _result = Sut.Interpolate(_person);
    }

    public SC06_InterpolatesFromDictionary() => ArrangeAndAct();

    [Then("the interpolated text should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesFromDictionary() => _result.Should().Be("Hello world, I'm John Smith");
}
