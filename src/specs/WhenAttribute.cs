namespace LowlandTech.Specs;

/// <summary>
/// Marks a method as the action trigger in a BDD-style test class.
/// Represents the Gherkin "When" clause. For Redux-like state machines,
/// this is where the action is dispatched to reduce the state.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class WhenAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the action description.
    /// This is the Gherkin "When" clause describing what triggers the behavior.
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets the action type for Redux-like state machines.
    /// This identifies the action dispatched to the reducer (e.g., "ADD_ITEM", "UPDATE_USER").
    /// </summary>
    public string? ActionType { get; set; }

    /// <summary>
    /// Gets or sets the action data/parameters in Gherkin-style format.
    /// This describes the parameters passed to the action (e.g., "Amount: 50", "userId: 123, role: 'admin'").
    /// </summary>
    public string? ActionData { get; set; }

    /// <summary>
    /// Returns a Gherkin-formatted string representation of this action.
    /// </summary>
    public override string ToString()
    {
        var action = Action ?? "(no action specified)";
        var typePart = !string.IsNullOrWhiteSpace(ActionType) ? $" ({ActionType})" : "";
        var dataPart = !string.IsNullOrWhiteSpace(ActionData) ? $" with {ActionData}" : "";

        return $"  When {action}{typePart}{dataPart}";
    }
}
