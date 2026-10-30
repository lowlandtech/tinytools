using LowlandTech.TinyTools.Testing;

namespace LowlandTech.TinyTools.Tests.SPEC3325.Snapshots.US01.Approvals;

[Trait(Spec.SPEC, "3325")]
[Trait(Spec.SC, "01")]
[UserStory("01", "Rendered output is pinned per case and changes are reviewable")]
public sealed class SC01_ApprovesRenderedOutput : IClassFixture<UseCases.SnapshotUseCase>
{
    private readonly UseCases.SnapshotUseCase useCase;
    private readonly TemplateSnapshotRunner runner = new();
    private string workspace = string.Empty;

    public SC01_ApprovesRenderedOutput(UseCases.SnapshotUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a workspace with an approved case, a changed case, an unapproved case and a broken template")]
    private void Given()
    {
        workspace = Path.Combine(Path.GetTempPath(), "tinytools-snap-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);

        File.WriteAllText(Path.Combine(workspace, "stable.tmpl"), "Hello Ada");
        File.WriteAllText(Path.Combine(workspace, "stable.approved.txt"), "Hello Ada");

        File.WriteAllText(Path.Combine(workspace, "changed.tmpl"), "Hello Grace");
        File.WriteAllText(Path.Combine(workspace, "changed.approved.txt"), "Hello Ada");

        File.WriteAllText(Path.Combine(workspace, "fresh.tmpl"), "Hello Cy");

        File.WriteAllText(Path.Combine(workspace, "broken.tmpl"), "@else");
        File.WriteAllText(Path.Combine(workspace, "broken.approved.txt"), "");
    }

    [When("the runner compares every case and then approves them")]
    private void When()
    {
        var cases = TemplateSnapshotRunner.Discover(workspace);
        useCase.Cases["Count"] = cases.Count;

        var checkedOnce = runner.Run(cases);
        useCase.Cases["StableOk"] = Find(checkedOnce, "stable").Ok;
        useCase.Cases["ChangedLine"] = Find(checkedOnce, "changed").FirstDifferingLine ?? -1;
        useCase.Cases["ChangedDiff"] = Find(checkedOnce, "changed").Diff;
        useCase.Cases["FreshMessage"] = Find(checkedOnce, "fresh").Message;
        useCase.Cases["BrokenMessage"] = Find(checkedOnce, "broken").Message;
        useCase.Cases["UntouchedAfterCheck"] = File.ReadAllText(Path.Combine(workspace, "changed.approved.txt"));

        var approved = runner.Run(cases, approve: true);
        useCase.Cases["StableMessage"] = Find(approved, "stable").Message;
        useCase.Cases["ChangedApproved"] = File.ReadAllText(Path.Combine(workspace, "changed.approved.txt"));
        useCase.Cases["FreshApproved"] = File.ReadAllText(Path.Combine(workspace, "fresh.approved.txt"));
    }

    private static SnapshotResult Find(IEnumerable<SnapshotResult> results, string name) =>
        results.First(result => result.Case.Name == name);

    [Then("the discovered case count should be 4")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_DiscoversEveryTemplate() => ((int)useCase.Cases["Count"]!).Should().Be(4);

    [Then("the stable flag should be true")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_PassesAMatchingCase() => ((bool)useCase.Cases["StableOk"]!).Should().BeTrue();

    [Then("the changed line should be 1")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ReportsTheFirstDifferingLine() => ((int)useCase.Cases["ChangedLine"]!).Should().Be(1);

    [Then("the changed diff should contain both values")]
    [Trait("UAC", "04"), Fact]
    public void UAC03b_ShowsBothSides() => ((string)useCase.Cases["ChangedDiff"]!).Should().Contain("-1: Hello Ada").And.Contain("+1: Hello Grace");

    [Then("the fresh message should contain the path")]
    [Trait("UAC", "05"), Fact]
    public void UAC04_NamesTheMissingApproval() => ((string)useCase.Cases["FreshMessage"]!).Should().Contain("fresh.approved.txt");

    [Then("the broken message should contain TS007")]
    [Trait("UAC", "06"), Fact]
    public void UAC05_ReportsTheSyntaxDiagnostic() => ((string)useCase.Cases["BrokenMessage"]!).Should().Contain("TS007");

    [Then("the untouched approval should be exact")]
    [Trait("UAC", "07"), Fact]
    public void UAC02b_NeverWritesWithoutApproval() => ((string)useCase.Cases["UntouchedAfterCheck"]!).Should().Be("Hello Ada");

    [Then("the changed approval should be updated")]
    [Trait("UAC", "08"), Fact]
    public void UAC03c_RewritesWhenApproved() => ((string)useCase.Cases["ChangedApproved"]!).Should().Be("Hello Grace");

    [Then("the fresh approval should be written")]
    [Trait("UAC", "09"), Fact]
    public void UAC04b_WritesAMissingApproval() => ((string)useCase.Cases["FreshApproved"]!).Should().Be("Hello Cy");

    [Then("the stable message should contain matches")]
    [Trait("UAC", "10"), Fact]
    public void UAC02c_LeavesMatchingApprovalsAlone() => ((string)useCase.Cases["StableMessage"]!).Should().Contain("matches");
}
