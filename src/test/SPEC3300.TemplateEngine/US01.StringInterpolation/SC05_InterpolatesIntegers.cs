namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "05")]
[UserStory("01", "String interpolation handles integer values")]
public class WhenInterpolatingIntegersTest : WhenTestingFor<string>
{
    private Person _person = null!;
    private string? _result;

    protected override string For()
    {
        return "Hello world, I'm ${Age} years old";
    }

    protected override void Given()
    {
        _person = new Person
        {
            Age = 20
        };
    }

    protected override void When()
    {
        _result = Sut.Interpolate(_person);
    }

    [Fact]
    public void ItShouldInterpolateAge()
    {
        _result.Should().Be("Hello world, I'm 20 years old");
    }
}