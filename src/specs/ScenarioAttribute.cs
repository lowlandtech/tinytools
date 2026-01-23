namespace LowlandTech.Specs;

/// <summary>
/// Marks a test class as a BDD scenario. Applied at the class level to provide
/// traceability metadata linking tests to specifications, user stories, and use cases.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ScenarioAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the specification identifier (e.g., "SPEC0001").
    /// </summary>
    public string? SpecId { get; set; }

    /// <summary>
    /// Gets or sets the user story identifier (e.g., "US01").
    /// </summary>
    public string? UserStory { get; set; }

    /// <summary>
    /// Gets or sets the use case identifier (e.g., "UC01").
    /// </summary>
    public string? UseCase { get; set; }

    /// <summary>
    /// Gets or sets the scenario identifier (e.g., "SC01").
    /// </summary>
    public string? ScenarioId { get; set; }

    /// <summary>
    /// Gets or sets the human-readable label describing this scenario.
    /// This is the Gherkin scenario title.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Returns a Gherkin-formatted string representation of this scenario.
    /// </summary>
    public override string ToString()
    {
        var tags = new List<string>();
        if (!string.IsNullOrWhiteSpace(SpecId)) tags.Add($"@{SpecId}");
        if (!string.IsNullOrWhiteSpace(UserStory)) tags.Add($"@{UserStory}");
        if (!string.IsNullOrWhiteSpace(UseCase)) tags.Add($"@{UseCase}");
        if (!string.IsNullOrWhiteSpace(ScenarioId)) tags.Add($"@{ScenarioId}");

        var tagLine = tags.Count > 0 ? string.Join(" ", tags) + "\n" : "";
        var label = Label ?? "Untitled Scenario";

        return $"{tagLine}Scenario: {label}";
    }
}
