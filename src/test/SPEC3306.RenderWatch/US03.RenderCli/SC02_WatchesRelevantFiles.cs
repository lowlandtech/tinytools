using LowlandTech.TinyTools.Rendering;

namespace LowlandTech.TinyTools.Tests.SPEC3306.RenderWatch.US03.RenderCli;

[Trait(Spec.SPEC, "3306")]
[Trait(Spec.SC, "02")]
[UserStory("03", "Render templates from files and refresh outputs when inputs change")]
public sealed class SC02_WatchesRelevantFiles : IClassFixture<UseCases.RenderCliUseCase>
{
    private readonly UseCases.RenderCliUseCase useCase;
    private string _root = string.Empty;

    public SC02_WatchesRelevantFiles(UseCases.RenderCliUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a watched template and model beside an unrelated file")]
    private void Given()
    {
        _root = Path.Combine(Path.GetTempPath(), "tinytools-watch-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "hello.tpl"), "Hello ${Context.Model.Name}!");
        File.WriteAllText(Path.Combine(_root, "model.json"), "{\"Name\":\"Ada\"}");
    }

    [When("an unrelated file changes, then the template changes, then the watch is cancelled")]
    private void When()
    {
        var renders = 0;
        var watcher = new TemplateWatcher(
            [Path.Combine(_root, "hello.tpl"), Path.Combine(_root, "model.json")],
            _ => renders++,
            TimeSpan.FromMilliseconds(20));

        using var cancellation = new CancellationTokenSource();
        var run = watcher.RunAsync(cancellation.Token);

        File.WriteAllText(Path.Combine(_root, "unrelated.txt"), "noise");
        Thread.Sleep(250);
        var afterUnrelated = renders;

        File.WriteAllText(Path.Combine(_root, "hello.tpl"), "Hello ${Context.Model.Name} again!");

        // A FileSystemWatcher notification is delivered by the operating system, so the wait is a
        // deadline rather than a fixed sleep: on a loaded Windows runner 200ms was not enough and
        // the scenario reported zero renders for a change that did arrive a moment later.
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (renders == 0 && DateTime.UtcNow < deadline)
        {
            Thread.Sleep(20);
        }

        cancellation.Cancel();

        try
        {
            run.GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            // Cancelling is the documented shutdown path.
        }

        useCase.Cases["UnrelatedRevisions"] = afterUnrelated;
        useCase.Cases["WatchRevisions"] = renders;
        useCase.Cases["WatchFiles"] = watcher.RelevantFiles.Count;
    }

    [Then("the unrelated revisions should be 0")]
    [Trait("UAC", "09"), Fact]
    public void UAC04_IgnoresUnrelatedChanges() => ((int)useCase.Cases["UnrelatedRevisions"]!).Should().Be(0);

    [Then("the watch revisions should be 1")]
    [Trait("UAC", "10"), Fact]
    public void UAC03_ReRendersOnRelevantChange() => ((int)useCase.Cases["WatchRevisions"]!).Should().Be(1);

    [Then("the watch files should have count 2")]
    [Trait("UAC", "11"), Fact]
    public void UAC04b_WatchesOnlyRelevantFiles() => ((int)useCase.Cases["WatchFiles"]!).Should().Be(2);
}
