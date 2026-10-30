namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "08")]
[UserStory("01", "String interpolation validates input parameters")]
public sealed class SC08_ThrowsOnNullModel : TinyToolsScenario<string>
{
    private Person? _person;
    private Action? _act;

    protected override string For() => "Hello world, I'm {FirstName} {LastName}";

    [Given("no model at all")]
    protected override void Given() => _person = null;

    [When("the template is interpolated with the null model")]
    protected override void When()
    {
        base.When();
        _act = () => Sut.Interpolate(_person!);
    }

    public SC08_ThrowsOnNullModel() => ArrangeAndAct();

    [Then("the interpolation should fail with ArgumentException")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ThrowsOnNullModel() => _act.Should().Throw<ArgumentException>();
}
