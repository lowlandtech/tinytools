namespace LowlandTech.TinyTools.UnitTests;

public class WhenInterpolatingDictionaryUntaggedTest : WhenTestingFor<string>
{
    private IDictionary _person = null!;
    private string? _result;

    protected override string For()
    {
        return "Hello world, I'm FirstName LastName";
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
        _result = Sut.Interpolate(_person, hasTags: false);
    }

    [Fact]
    public void ItShouldInterpolateFirstAndLastName()
    {
        _result.Should().Be("Hello world, I'm John Smith");
    }
}
