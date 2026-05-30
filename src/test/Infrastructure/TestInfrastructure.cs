namespace LowlandTech.TinyTools.Tests.Infrastructure;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class UserStoryAttribute : Attribute
{
    public UserStoryAttribute(string id, string description)
    {
        Id = id;
        Description = description;
    }

    public string Id { get; }

    public string Description { get; }
}

public static class Spec
{
    public const string SPEC = nameof(SPEC);
    public const string SC = nameof(SC);
}