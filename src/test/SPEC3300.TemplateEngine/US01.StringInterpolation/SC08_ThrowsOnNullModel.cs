namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "08")]
[UserStory("01", "String interpolation validates input parameters")]
public class WhenInterpolatingNullModelTest : WhenTestingFor<string>
{
    private Person? _person;
    private Action? _act;

    protected override string For()
    {
        return "Hello world, I'm {FirstName} {LastName}";

    }

    protected override void Given()
    {
        _person = null;
    }

    protected override void When()
    {
        _act = () => Sut.Interpolate(_person);
    }

    [Fact]
    public void ItShouldThrowAModelArgumentException()
    {
        _act.Should()
            .Throw<ArgumentException>();
    }
}
