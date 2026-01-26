namespace LowlandTech.TinyTools.UnitTests;

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
