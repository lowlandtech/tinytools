namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "05")]
[UserStory("01", "String interpolation handles integer values")]
public sealed class SC05_InterpolatesIntegers : TinyToolsScenario<string>
{
    private Person _person = null!;
    private string? _result;

    protected override string For() => "Hello world, I'm ${Age} years old";

    [Given("a person aged twenty")]
    protected override void Given() => _person = new Person { Age = 20 };

    [When("the template is interpolated with the person")]
    protected override void When()
    {
        base.When();
        _result = Sut.Interpolate(_person);
    }

    public SC05_InterpolatesIntegers() => ArrangeAndAct();

    [Then("the interpolated text should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesAge() => _result.Should().Be("Hello world, I'm 20 years old");
}
