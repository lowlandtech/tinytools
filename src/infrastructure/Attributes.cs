using Xunit.v3;

namespace LowlandTech.TinyTools.Tests.Infrastructure;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class GivenAttribute(string description) : Attribute, ITraitAttribute
{
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => [new("Given", description)];
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class WhenAttribute(string description) : Attribute, ITraitAttribute
{
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => [new("When", description)];
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ThenAttribute(string description) : Attribute, ITraitAttribute
{
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => [new("Then", description)];
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class UserStoryAttribute(string id, string description) : Attribute, ITraitAttribute
{
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => [new(Spec.US, id), new("Story", description)];
}
