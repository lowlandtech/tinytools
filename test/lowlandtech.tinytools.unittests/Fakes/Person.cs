using System;

namespace LowlandTech.TinyTools.Test.Fakes
{
    internal class Person
    {
        public string FirstName { get; internal set; }
        public string LastName { get; internal set; }
        public int Age { get; internal set; }
        public bool IsMarried { get; internal set; }
        public DateTime Dob { get; internal set; }
    }
}
