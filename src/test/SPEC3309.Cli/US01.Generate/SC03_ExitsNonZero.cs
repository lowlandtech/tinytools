using System.Diagnostics;

namespace LowlandTech.TinyTools.Tests.SPEC3309.Cli.US01.Generate;

[Trait(Spec.SPEC, "3309")]
[Trait(Spec.SC, "03")]
[UserStory("01", "The generate command exits non-zero instead of reporting success")]
public sealed class SC03_ExitsNonZero : IClassFixture<UseCases.GenerateUseCase>
{
    private readonly UseCases.GenerateUseCase useCase;
    private string _root = string.Empty;

    public SC03_ExitsNonZero(UseCases.GenerateUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "tinytools.slnx")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not reachable.");
    }

    private static (int ExitCode, string Error) RunCli(string root, string workflowFile)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(Path.Combine(RepositoryRoot(), "src", "cli", "LowlandTech.TinyTools.Cli.csproj"));
        startInfo.ArgumentList.Add("--");
        startInfo.ArgumentList.Add("generate");
        startInfo.ArgumentList.Add(workflowFile);

        using var process = Process.Start(startInfo)!;
        var errorTask = process.StandardError.ReadToEndAsync();
        var outputTask = process.StandardOutput.ReadToEndAsync();
        process.WaitForExit(120_000);
        return (process.ExitCode, (errorTask.GetAwaiter().GetResult() + outputTask.GetAwaiter().GetResult()).Trim());
    }

    [Given("two workflows, one naming an untrusted command and one whose command fails")]
    private void Given()
    {
        _root = Path.Combine(Path.GetTempPath(), "tinytools-cli-exit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        File.WriteAllText(
            Path.Combine(_root, "refused.yml"),
            "steps:\n  - id: cmd\n    type: command\n    command: rm\n    args: [\"-rf\", \".\"]\n");
        File.WriteAllText(
            Path.Combine(_root, "failing.yml"),
            "steps:\n  - id: cmd\n    type: command\n    command: git\n    args: [\"rev-parse\", \"--verify\", \"definitely-not-a-ref\"]\n");
    }

    [When("the CLI entry point runs each workflow once")]
    private void When()
    {
        var refused = RunCli(_root, Path.Combine(_root, "refused.yml"));
        useCase.Cases["RefusedExit"] = refused.ExitCode;
        useCase.Cases["RefusedError"] = refused.Error;

        var failing = RunCli(_root, Path.Combine(_root, "failing.yml"));
        useCase.Cases["FailingExit"] = failing.ExitCode;
        useCase.Cases["FailingError"] = failing.Error;
    }

    [Then("the refused exit should be 3")]
    [Trait("UAC", "01"), Fact]
    public void UAC02_RefusesWithItsOwnExitCode() => ((int)useCase.Cases["RefusedExit"]!).Should().Be(3);

    [Then("the refused error should contain trusted command list")]
    [Trait("UAC", "02"), Fact]
    public void UAC02b_NamesThePolicyInTheMessage() => ((string)useCase.Cases["RefusedError"]!).Should().Contain("trusted command list");

    [Then("the failing exit should be 1")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ExitsNonZeroOnCommandFailure() => ((int)useCase.Cases["FailingExit"]!).Should().Be(1);

    [Then("the failing error should contain failed")]
    [Trait("UAC", "04"), Fact]
    public void UAC03b_ReportsTheChildFailure() => ((string)useCase.Cases["FailingError"]!).Should().Contain("failed");
}
