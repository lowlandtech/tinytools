﻿namespace LowlandTech.TinyTools.UnitTests;

public class WhenInterpolatingStringsTest : WhenTestingFor<string>
{
    private Person _person = null!;
    private string? _result;

    protected override string For()
    {
        return "Hello world, I'm {FirstName} {LastName}";
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
        _result = Sut.Interpolate(_person);
    }

    [Fact]
    public void ItShouldInterpolateFirstAndLastName()
    {
        _result.Should().Be("Hello world, I'm John Smith");
    }
}
