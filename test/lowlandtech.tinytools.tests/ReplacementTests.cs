using Shouldly;
using System;
using Xunit;

namespace TinyTools.Test
{
    public class ReplacementTests
    {
        [Fact]
        public void ShouldReplaceStrings()
        {
            var person = new Person
            {
                FirstName = "John",
                LastName = "Smith"
            };
            var template = "Hello world, I'm {FirstName} {LastName}";

            var result = template.Interpolate(person);
            result.ShouldBe("Hello world, I'm John Smith");
        }

        [Fact]
        public void ShouldReplaceIntegers()
        {
            var person = new Person
            {
                Age = 20
            };
            var template = "Hello world, I'm {Age} years old";

            var result = template.Interpolate(person);
            result.ShouldBe("Hello world, I'm 20 years old");
        }

        [Fact]
        public void ShouldReplaceBooleans()
        {
            var person = new Person
            {
                IsMarried = true
            };
            var template = "Hello world, married: {IsMarried}";

            var result = template.Interpolate(person);
            result.ShouldBe("Hello world, married: True");
        }

        [Fact]
        public void ShouldReplaceDate()
        {
            var person = new Person
            {
                Dob = new DateTime(2001, 1, 25)
            };
            var template = "Hello world, date of birth: {Dob}";

            var result = template.Interpolate(person);
            result.ShouldBe("Hello world, date of birth: 01/25/2001 00:00:00");
        }
    }
}
