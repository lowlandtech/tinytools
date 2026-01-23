using Xunit;
using Xunit.v3;

namespace LowlandTech.Specs;

/// <summary>
/// Marks a method as a test assertion in a BDD-style test class.
/// This attribute inherits from <see cref="FactAttribute"/> so xUnit
/// automatically discovers and executes these methods as tests.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ThenAttribute : FactAttribute, ITraitAttribute
{
    /// <summary>
    /// Gets or sets the UAT (User Acceptance Test) identifier for this test assertion.
    /// This value is exposed as a trait with key "Uat".
    /// </summary>
    public string? Uat { get; set; }

    /// <summary>
    /// Gets or sets a comment describing the purpose or context of this assertion.
    /// This is rendered as a Gherkin comment above the Then clause.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the assertion text for this test.
    /// This is the Gherkin "Then" clause describing what must hold true.
    /// This value is used as the test's display name.
    /// </summary>
    public string? Assertion
    {
        get => DisplayName;
        set => DisplayName = value;
    }

    /// <summary>
    /// Gets or sets the property name on the state being validated.
    /// </summary>
    public string? Property { get; set; }

    /// <summary>
    /// Gets or sets the comparison operator for the assertion.
    /// Examples: "Equals", "GreaterThan", "Contains", "IsNull", "IsNotNull".
    /// </summary>
    public string? Operator { get; set; }

    /// <summary>
    /// Gets or sets the expected value for the assertion.
    /// </summary>
    public object? Expected { get; set; }

    /// <summary>
    /// Gets or sets the postcondition property name that should hold after the action.
    /// </summary>
    public string? PostconditionProperty { get; set; }

    /// <summary>
    /// Gets or sets the postcondition operator.
    /// </summary>
    public string? PostconditionOperator { get; set; }

    /// <summary>
    /// Gets or sets the postcondition expected value.
    /// </summary>
    public object? PostconditionExpected { get; set; }

    /// <inheritdoc />
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
    {
        var traits = new List<KeyValuePair<string, string>>();

        if (!string.IsNullOrWhiteSpace(Uat))
        {
            traits.Add(new KeyValuePair<string, string>("Uat", Uat));
        }

        return traits;
    }

    /// <summary>
    /// Returns a Gherkin-formatted string representation of this assertion.
    /// </summary>
    public override string ToString()
    {
        var lines = new List<string>();

        // Comment line with UAT identifier
        if (!string.IsNullOrWhiteSpace(Uat) || !string.IsNullOrWhiteSpace(Comment))
        {
            var uatPart = !string.IsNullOrWhiteSpace(Uat) ? $"UAT-{Uat}" : "";
            var commentPart = Comment ?? "";
            
            if (!string.IsNullOrWhiteSpace(uatPart) && !string.IsNullOrWhiteSpace(commentPart))
            {
                lines.Add($"  #{uatPart}: {commentPart}");
            }
            else if (!string.IsNullOrWhiteSpace(uatPart))
            {
                lines.Add($"  #{uatPart}");
            }
            else
            {
                lines.Add($"  # {commentPart}");
            }
        }

        // Then line
        var thenParts = new List<string> { "  Then" };
        if (!string.IsNullOrWhiteSpace(Property) && !string.IsNullOrWhiteSpace(Operator))
        {
            var val = Expected is string s ? $"\"{s}\"" : Expected?.ToString() ?? "null";
            thenParts.Add($"{Property} {Operator} {val}");
        }
        else if (!string.IsNullOrWhiteSpace(Assertion))
        {
            thenParts.Add(Assertion);
        }
        lines.Add(string.Join(" ", thenParts));

        // Postcondition line
        if (!string.IsNullOrWhiteSpace(PostconditionProperty) && !string.IsNullOrWhiteSpace(PostconditionOperator))
        {
            var postVal = PostconditionExpected is string ps ? $"\"{ps}\"" : PostconditionExpected?.ToString() ?? "null";
            lines.Add($"    And {PostconditionProperty} {PostconditionOperator} {postVal}");
        }

        return string.Join("\n", lines);
    }
}
