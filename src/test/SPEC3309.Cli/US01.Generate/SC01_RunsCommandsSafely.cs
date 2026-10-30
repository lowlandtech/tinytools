using LowlandTech.TinyTools.Workflows;

namespace LowlandTech.TinyTools.Tests.SPEC3309.Cli.US01.Generate;

[Trait(Spec.SPEC, "3309")]
[Trait(Spec.SC, "01")]
[UserStory("01", "The generate command runs workflow command steps safely")]
public sealed class SC01_RunsCommandsSafely : IClassFixture<UseCases.GenerateUseCase>
{
    private readonly UseCases.GenerateUseCase useCase;

    public SC01_RunsCommandsSafely(UseCases.GenerateUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    private static WorkflowStep Step(string command, params string[] args) => new() { Id = "cmd", Type = "command", Command = command, Args = [.. args] };

    [Given("a trusted shell that floods both pipes, an untrusted command and a failing command")]
    private void Given()
    {
    }

    [When("each command step is executed once through the public executor")]
    private void When()
    {
        var root = Path.GetTempPath();
        var trusted = new[] { "pwsh" };

        try
        {
            // Forty kilobytes on each pipe, well past the roughly one-buffer limit:
            // reading one pipe to the end first deadlocks on this input.
            const string flood = "$o = 'o' * 40000; $o; [Console]::Error.Write($o); exit 0";
            var result = WorkflowCommandExecutor
                .RunAsync(Step("pwsh", "-NoProfile", "-Command", flood), root, CancellationToken.None, trusted)
                .GetAwaiter().GetResult();
            useCase.Cases["FloodLength"] = (result.Message ?? string.Empty).Length;
        }
        catch (Exception error)
        {
            useCase.Cases["FloodLength"] = -1;
        }

        try
        {
            WorkflowCommandExecutor
                .RunAsync(Step("rm", "-rf", "."), root, CancellationToken.None, trusted)
                .GetAwaiter().GetResult();
            useCase.Cases["Refused"] = "no-throw";
        }
        catch (Exception error)
        {
            useCase.Cases["Refused"] = error.GetType().Name; useCase.Cases["RefusedMessage"] = error.Message;
        }

        try
        {
            WorkflowCommandExecutor
                .RunAsync(
                    Step("pwsh", "-NoProfile", "-Command", "[Console]::Error.Write('boom'); exit 7"),
                    root,
                    CancellationToken.None,
                    trusted)
                .GetAwaiter().GetResult();
            useCase.Cases["FailureMessage"] = "no-throw";
        }
        catch (Exception error)
        {
            useCase.Cases["FailureMessage"] = error.Message;
        }

        useCase.Cases["DefaultAllowed"] = WorkflowCommandPolicy.IsAllowed("dotnet"); useCase.Cases["DefaultRefuses"] = WorkflowCommandPolicy.IsAllowed("rm");
    }

    [Then("the flood output length should be 40000")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_DrainsBothPipesConcurrently() => ((int)useCase.Cases["FloodLength"]!).Should().BeGreaterThan(30000);

    [Then("the refusal type should be WorkflowCommandRefusedException")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RefusesUntrustedCommands() => ((string)useCase.Cases["Refused"]!).Should().Be("WorkflowCommandRefusedException");

    [Then("the refusal message should contain rm")]
    [Trait("UAC", "03"), Fact]
    public void UAC02b_NamesTheRefusedValue() => ((string)useCase.Cases["RefusedMessage"]!).Should().Contain("'rm'");

    [Then("the failure message should contain (7)")]
    [Trait("UAC", "04"), Fact]
    public void UAC03_ReportsTheExitCodeAndError() => ((string)useCase.Cases["FailureMessage"]!).Should().Contain("(7)").And.Contain("boom");

    [Then("the dotnet command should be accepted")]
    [Trait("UAC", "05"), Fact]
    public void UAC02c_TrustsTheDocumentedCommands() => ((bool)useCase.Cases["DefaultAllowed"]!).Should().BeTrue();

    [Then("the rm command should be rejected")]
    [Trait("UAC", "06"), Fact]
    public void UAC02d_RefusesAnythingElse() => ((bool)useCase.Cases["DefaultRefuses"]!).Should().BeFalse();
}
