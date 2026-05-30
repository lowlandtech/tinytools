namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US01.StringInterpolation;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "06")]
[UserStory("01", "String interpolation replaces variables from dictionaries")]
public class WhenInterpolatingDictionaryTest : WhenTestingFor<string>
{
    private IDictionary _person = null!;
    private string? _result;

    protected override string For()
    {
        return "Hello world, I'm ${FirstName} ${LastName}";
    }

    protected override void Given()
    {
        _person = new Dictionary<string, string>()
        {
            { "FirstName","John" },
            { "LastName","Smith" }
        };
    }

    protected override void When()
    {
        _result = Sut.Interpolate(_person);
    }

    [Fact]
    public void ItShouldInterpolateFirstAndLastName()
    {
        _result.Should().Be("Hello world, I'm John Smith");
    }
}
