namespace LowlandTech.TinyTools.UnitTests.Fakes;

internal class Person
{
    public string FirstName { get; internal set; } = null!;
    public string LastName { get; internal set; } = null!;
    public int Age { get; internal set; }
    public bool IsMarried { get; internal set; }
    public DateTime Dob { get; internal set; }
}
