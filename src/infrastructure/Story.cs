using Xunit.v3;

namespace LowlandTech.TinyTools.Tests.Infrastructure;

/// <summary>
/// Names the caller goal of one user story and the behavior cases its scenarios
/// assert. The vocabulary is the contract the test-specification layout check
/// enforces, so a fact cannot claim a case the story never defined.
/// </summary>
public abstract class Story
{
    /// <summary>The caller's goal, phrased as what a user of the library wants.</summary>
    public abstract string Goal { get; }

    /// <summary>Case identity to the behavior that case represents.</summary>
    public abstract IReadOnlyDictionary<string, string> Cases { get; }

    public string Describe(string caseId) =>
        Cases.TryGetValue(caseId, out var description)
            ? description
            : throw new ArgumentOutOfRangeException(nameof(caseId), caseId, "The case is not part of this story.");
}

/// <summary>
/// Story vocabulary backed by an enum, so a scenario fact names its case with
/// <see cref="CaseAttribute{TCase}"/> and the compiler rejects an invented one.
/// </summary>
public abstract class Story<TCase> : Story where TCase : struct, Enum
{
    protected abstract IReadOnlyDictionary<TCase, string> Vocabulary { get; }

    public override IReadOnlyDictionary<string, string> Cases =>
        Vocabulary.ToDictionary(pair => Identify(pair.Key), pair => pair.Value);

    public static string Identify(TCase value) => typeof(TCase).Name + "." + value;
}

/// <summary>Declares which story a scenario belongs to.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class StoryAttribute(Type storyType) : Attribute
{
    public Type StoryType { get; } = storyType;

    public Story Create() => (Story)Activator.CreateInstance(StoryType)!;
}

/// <summary>Case identity attached to a fact, read by the layout check.</summary>
public interface ICaseAnnotation
{
    string CaseId { get; }
}

/// <summary>
/// Names the single behavior case a fact proves, and surfaces it as a trait so
/// a case can be selected with <c>--filter Case=...</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class CaseAttribute<TCase>(TCase value) : Attribute, ICaseAnnotation, ITraitAttribute
    where TCase : struct, Enum
{
    public TCase Value { get; } = value;

    public string CaseId => Story<TCase>.Identify(Value);

    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
        [new KeyValuePair<string, string>("Case", CaseId)];
}
