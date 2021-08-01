using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace LowlandTech.TinyTools.UnitTests
{
    public class WhenInterpolatingDictionaryTest : WhenTestingFor<string>
    {
        private IDictionary _person;
        private string _result;

        protected override string For()
        {
            return "Hello world, I'm {FirstName} {LastName}";
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
}
