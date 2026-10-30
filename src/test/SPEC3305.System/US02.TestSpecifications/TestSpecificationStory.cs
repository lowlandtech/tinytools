namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US02.TestSpecifications;

/// <summary>Behavior cases for the test-specification layout story.</summary>
public enum TestSpecificationCase
{
    MigratedScenariosDeclareCases,
    InventoryCountsReconcile,
}

/// <summary>
/// Story: a maintainer can tell which behavior each fact proves without reading
/// the fact body, because every migrated scenario names its goal and cases.
/// </summary>
public sealed class TestSpecificationStory : Story<TestSpecificationCase>
{
    public override string Goal =>
        "A maintainer reads the suite and knows which behavior each fact proves and which scenarios are still to migrate.";

    protected override IReadOnlyDictionary<TestSpecificationCase, string> Vocabulary =>
        new Dictionary<TestSpecificationCase, string>
        {
            [TestSpecificationCase.MigratedScenariosDeclareCases] =
                "every migrated scenario declares one known case per fact and keeps its acceptance identity",
            [TestSpecificationCase.InventoryCountsReconcile] =
                "migrated and unmigrated scenarios together account for every discovered scenario",
        };
}

/// <summary>The observed layout state of the test assembly.</summary>
public sealed record TestSpecificationResult(
    IReadOnlyList<string> Violations,
    IReadOnlyList<Type> Migrated,
    IReadOnlyList<Type> Unmigrated,
    IReadOnlyList<Type> Scenarios)
{
    public bool IsConsistent => Violations.Count == 0;

    public bool InventoryReconciles => Migrated.Count + Unmigrated.Count == Scenarios.Count;
}
