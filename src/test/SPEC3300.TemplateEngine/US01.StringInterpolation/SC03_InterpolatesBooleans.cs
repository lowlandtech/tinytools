namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "03")]
[UserStory("01", "String interpolation handles boolean values")]
public sealed class SC03_InterpolatesBooleans : TinyToolsScenario<string>
{
    private Person _person = null!;
    private string? _result;

    protected override string For() => "Hello world, married: ${IsMarried}";

    [Given("a person who is married")]
    protected override void Given() => _person = new Person { IsMarried = true };

    [When("the template is interpolated with the person")]
    protected override void When()
    {
        base.When();
        _result = Sut.Interpolate(_person);
    }

    public SC03_InterpolatesBooleans() => ArrangeAndAct();

    [Then("the interpolated text should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_InterpolatesIsMarried() => _result.Should().Be("Hello world, married: True");
}
