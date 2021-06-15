using Shouldly;
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

    }

    internal class Person
    {
        public Person()
        {
        }

        public string FirstName { get; internal set; }
        public string LastName { get; internal set; }
        public int Age { get; internal set; }
    }
}
