namespace LowlandTech.Specs;

/// <summary>
/// Marks a method as the precondition setup in a BDD-style test class.
/// Represents the Gherkin "Given" clause with structured validation data.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class GivenAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the property name on the state being validated as a precondition.
    /// </summary>
    public string? Property { get; set; }

    /// <summary>
    /// Gets or sets the comparison operator for the precondition validation.
    /// Examples: "Equals", "GreaterThan", "Contains", "IsNull", "IsNotNull".
    /// </summary>
    public string? Operator { get; set; }

    /// <summary>
    /// Gets or sets the expected value for the precondition validation.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Returns a Gherkin-formatted string representation of this precondition.
    /// Use <see cref="ToString(bool)"/> when rendering multiple preconditions.
    /// </summary>
    public override string ToString() => ToString(isFirst: true);

    /// <summary>
    /// Returns a Gherkin-formatted string representation of this precondition.
    /// </summary>
    /// <param name="isFirst">True to render as "Given", false to render as "And".</param>
    public string ToString(bool isFirst)
    {
        var keyword = isFirst ? "Given" : "And";

        if (string.IsNullOrWhiteSpace(Property))
            return $"  {keyword} (no precondition specified)";

        var op = Operator ?? "Equals";
        var val = Value is string s ? $"\"{s}\"" : Value?.ToString() ?? "null";

        return $"  {keyword} {Property} {op} {val}";
    }
}
