namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "03")]
[UserStory("01", "String interpolation handles boolean values")]
public class WhenInterpolatingBooleansTest : WhenTestingFor<string>
{
    private Person _person = null!;
    private string? _result;

    protected override string For()
    {
        return "Hello world, married: ${IsMarried}";
    }

    protected override void Given()
    {
        _person = new Person
        {
            IsMarried = true
        };
    }

    protected override void When()
    {
        _result = Sut.Interpolate(_person);
    }

    [Fact]
    public void ItShouldInterpolateIsMarried()
    {
        _result.Should().Be("Hello world, married: True");
    }
}
