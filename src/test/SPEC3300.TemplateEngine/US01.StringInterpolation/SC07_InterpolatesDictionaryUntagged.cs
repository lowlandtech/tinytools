namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "07")]
[UserStory("01", "String interpolation replaces dictionary values without tags")]
public sealed class SC07_InterpolatesDictionaryUntagged : TinyToolsScenario<string>
{
    private IDictionary _person = null!;
    private string? _result;

    protected override string For() => "Hello world, I'm FirstName LastName";

    [Given("a dictionary holding a first and last name")]
    protected override void Given() => _person = new Dictionary<string, string>
    {
        { "FirstName", "John" },
        { "LastName", "Smith" },
    };

    [When("the dictionary template is interpolated without tags")]
    protected override void When()
    {
        base.When();
        _result = Sut.Interpolate(_person, hasTags: false);
    }

    public SC07_InterpolatesDictionaryUntagged() => ArrangeAndAct();

    [Then("the interpolated text should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesDictionaryUntagged() => _result.Should().Be("Hello world, I'm John Smith");
}
