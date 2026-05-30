namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "09")]
[UserStory("01", "String interpolation validates input parameters")]
public class WhenInterpolatingEmptyStringsTest : WhenTestingFor<string>
{
    private Person _person = null!;
    private Action? _act;

    protected override string For()
    {
        return "";
    }

    protected override void Given()
    {
        _person = new Person
        {
            FirstName = "John",
            LastName = "Smith"
        };
    }

    protected override void When()
    {
        _act = () => Sut.Interpolate(_person);
    }

    [Fact]
    public void ItShouldThrowATemplateArgumentException()
    {
        _act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Template should be supplied. (Parameter 'template')");
    }
}
