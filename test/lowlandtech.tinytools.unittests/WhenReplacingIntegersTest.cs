using FluentAssertions;
using LowlandTech.TinyTools.Test.Fakes;
using Xunit;

namespace LowlandTech.TinyTools.Tests
{
    public class WhenInterpolatingIntegersTest : WhenTestingFor<string>
    {
        private Person _person;
        private string _result;

        protected override string For()
        {
            return "Hello world, I'm {Age} years old";
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
}