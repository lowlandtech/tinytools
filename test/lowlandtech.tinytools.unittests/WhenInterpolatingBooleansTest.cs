using FluentAssertions;
using LowlandTech.TinyTools.UnitTests.Fakes;
using Xunit;

namespace LowlandTech.TinyTools.UnitTests
{
    public class WhenInterpolatingBooleansTest : WhenTestingFor<string>
    {
        private Person _person;
        private string _result;

        protected override string For()
        {
            return "Hello world, married: {IsMarried}";
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
}
