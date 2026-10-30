using System.Diagnostics;
using System.Text.Json;

namespace LowlandTech.TinyTools.Tests.SPEC3320.Gates.US01.Vendored;

[Trait(Spec.SPEC, "3320")]
[Trait(Spec.SC, "01")]
[UserStory("01", "CI enforces the layout and roadmap gates from inside the repository")]
public sealed class SC01_RunsTheGates : IClassFixture<UseCases.VendoredGatesUseCase>
{
    private readonly UseCases.VendoredGatesUseCase useCase;

    public SC01_RunsTheGates(UseCases.VendoredGatesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("the vendored layout and roadmap gates inside eng")]
    private void Given()
    {
    }

    [When("each gate runs against this repository and against a mutated board")]
    private void When()
    {
        var root = UseCases.VendoredGatesUseCase.RepositoryRoot();
        useCase.Cases["LayoutExists"] = File.Exists(Path.Combine(root, "eng", "verify-test-layout.py"));
        useCase.Cases["RoadmapExists"] = File.Exists(Path.Combine(root, "eng", "validate_roadmap.py"));

        var layout = UseCases.VendoredGatesUseCase.Run("eng/verify-test-layout.py", ["--root", "src/test", "--json"], root);
        useCase.Cases["LayoutExit"] = layout.Exit;
        useCase.Cases["LayoutViolations"] = UseCases.VendoredGatesUseCase.CountViolations(layout.Output);

        var roadmap = UseCases.VendoredGatesUseCase.Run("eng/validate_roadmap.py", ["--repository-root", "."], root);
        useCase.Cases["RoadmapExit"] = roadmap.Exit;

        var broken = Path.Combine(Path.GetTempPath(), "tinytools-gate-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(broken, "docs", "workitems", "done"));
        File.WriteAllText(
            Path.Combine(broken, "docs", "workitems", "README.md"),
            "| ID | Scope | Title | Priority | Owner | Depends on | Status | Refined | Issue |\n|---|---|---|---|---|---|---|---|---|\n");
        useCase.Cases["MutatedExit"] = UseCases.VendoredGatesUseCase.Run("eng/validate_roadmap.py", ["--repository-root", broken], root).Exit;
    }

    [Then("the layout gate file should be true")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_VendorsTheLayoutGate() => ((bool)useCase.Cases["LayoutExists"]!).Should().BeTrue();

    [Then("the roadmap gate file should be true")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_VendorsTheRoadmapGate() => ((bool)useCase.Cases["RoadmapExists"]!).Should().BeTrue();

    [Then("the layout violations should be 0")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_PassesTheLayoutGate() => ((int)useCase.Cases["LayoutViolations"]!).Should().Be(0);

    [Then("the layout exit should be 0")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ExitsZeroOnACleanTree() => ((int)useCase.Cases["LayoutExit"]!).Should().Be(0);

    [Then("the roadmap exit should be 0")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_PassesTheRoadmapGate() => ((int)useCase.Cases["RoadmapExit"]!).Should().Be(0);

    [Then("the mutated exit should be refused")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_FailsOnAMutatedBoard() => ((int)useCase.Cases["MutatedExit"]!).Should().NotBe(0);
}
