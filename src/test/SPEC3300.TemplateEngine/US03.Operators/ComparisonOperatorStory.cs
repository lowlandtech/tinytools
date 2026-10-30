namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US03.Operators;

/// <summary>Behavior cases for the comparison-operator story.</summary>
public enum ComparisonOperatorCase
{
    GreaterOrEqualSatisfied,
    LessThanSatisfied,
    LessOrEqualSatisfied,
    GreaterThanSatisfied,
}

/// <summary>
/// Story: a template author compares model values inside <c>@if</c> conditions.
/// </summary>
public sealed class ComparisonOperatorStory : Story<ComparisonOperatorCase>
{
    public override string Goal =>
        "A template author compares model values inside @if conditions and gets the branch each comparison implies.";

    protected override IReadOnlyDictionary<ComparisonOperatorCase, string> Vocabulary =>
        new Dictionary<ComparisonOperatorCase, string>
        {
            [ComparisonOperatorCase.GreaterOrEqualSatisfied] = "an age of 25 satisfies the >= 21 rule and renders its body",
            [ComparisonOperatorCase.LessThanSatisfied] = "an age of 25 satisfies the < 65 rule and renders its body",
            [ComparisonOperatorCase.LessOrEqualSatisfied] = "a balance of 100 satisfies the <= 100 rule and renders its body",
            [ComparisonOperatorCase.GreaterThanSatisfied] = "an item count of 5 satisfies the > 3 rule and renders its body",
        };
}

/// <summary>
/// The observed outcome of rendering the comparison template: which of the four
/// comparison bodies appeared.
/// </summary>
public sealed record ComparisonOperatorResult(string Rendered)
{
    public static ComparisonOperatorResult From(string rendered) => new(rendered);

    public bool GreaterOrEqualRendered =>
        Rendered.Contains("You can purchase alcohol.", StringComparison.Ordinal);

    public bool LessThanRendered =>
        Rendered.Contains("Not yet eligible for senior discount.", StringComparison.Ordinal);

    public bool LessOrEqualRendered =>
        Rendered.Contains("Low balance warning.", StringComparison.Ordinal);

    public bool GreaterThanRendered =>
        Rendered.Contains("Bulk discount applied!", StringComparison.Ordinal);
}
