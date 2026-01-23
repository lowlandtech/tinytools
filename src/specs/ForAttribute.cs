namespace LowlandTech.Specs;

/// <summary>
/// Marks a method as the state/SUT binding in a BDD-style test class.
/// The For method returns the system under test that modifies state.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ForAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the Type of the state record that the SUT operates on.
    /// This is the actual state shape being modified by the reducer/SUT.
    /// </summary>
    public Type? StateType { get; set; }

    /// <summary>
    /// Returns a Gherkin-formatted comment for the state binding.
    /// </summary>
    public override string ToString()
    {
        var stateName = StateType?.Name ?? "UnknownState";
        return $"  # For: {stateName}";
    }
}
