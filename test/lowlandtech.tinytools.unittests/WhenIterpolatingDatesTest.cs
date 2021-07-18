using FluentAssertions;
using LowlandTech.TinyTools.Test.Fakes;
using System;
using Xunit;

namespace LowlandTech.TinyTools.Tests
{
    public class WhenInterpolatingDatesTest : WhenTestingFor<string>
    {
        private Person _person;
        private string _result;

        protected override string For()
        {
            return "Hello world, date of birth: {Dob}";
        }

        protected override void Given()
        {
            _person = new Person
            {
                Dob = new DateTime(2001, 1, 25)
            };
        }

        protected override void When()
        {
            _result = Sut.Interpolate(_person);
        }

        [Fact]
        public void ShouldIterpolateDateOfbBirth()
        {
            _result.Should().Be("Hello world, date of birth: " + _person.Dob);
        }
    }
}
