using LowlandTech.TinyTools.Rendering;

namespace LowlandTech.TinyTools.Tests.SPEC3306.RenderWatch.US03.RenderCli;

[Trait(Spec.SPEC, "3306")]
[Trait(Spec.SC, "01")]
[UserStory("03", "Render templates from files and refresh outputs when inputs change")]
public sealed class SC01_RendersAndWatchesFiles : IClassFixture<UseCases.RenderCliUseCase>
{
    private readonly UseCases.RenderCliUseCase useCase;

    public SC01_RendersAndWatchesFiles(UseCases.RenderCliUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    private string _root = string.Empty;

    [Given("a workspace with a template, a model, a broken template, an unreadable model and a watched pair")]
    private void Given()
    {
        _root = Path.Combine(Path.GetTempPath(), "tinytools-render-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "hello.tpl"), "Hello ${Context.Model.Name}!");
        File.WriteAllText(Path.Combine(_root, "model.json"), "{\"Name\":\"Ada\"}");
        File.WriteAllText(Path.Combine(_root, "broken.tpl"), "@service(\"nope\", 1)");
        File.WriteAllText(Path.Combine(_root, "bad.json"), "{ not json");
    }

    [When("render, batch, refusal and watch each run once")]
    private void When()
    {
        var renderer = new TemplateFileRenderer();
        var template = Path.Combine(_root, "hello.tpl");
        var model = Path.Combine(_root, "model.json");

        useCase.Cases["Rendered"] = renderer.Render(template, model, "out/hello.txt", _root);
        useCase.Cases["RenderedText"] = File.ReadAllText(Path.Combine(_root, "out", "hello.txt"));

        File.WriteAllText(Path.Combine(_root, "out", "keep.txt"), "original");
        useCase.Cases["Broken"] = renderer.Render(Path.Combine(_root, "broken.tpl"), model, "out/keep.txt", _root);
        useCase.Cases["KeptText"] = File.ReadAllText(Path.Combine(_root, "out", "keep.txt"));

        useCase.Cases["BadModel"] = renderer.Render(template, Path.Combine(_root, "bad.json"), "out/bad.txt", _root);

        var batch = renderer.RenderBatch(
            [
                (template, model, "out/one.txt"),
                (Path.Combine(_root, "broken.tpl"), model, "out/two.txt"),
                (template, model, "out/three.txt"),
            ],
            _root);
        useCase.Cases["BatchCount"] = batch.Count;
        useCase.Cases["BatchOrder"] = string.Join(",", batch.Select(result => Path.GetFileName(result.OutputPath ?? "failed")));
        useCase.Cases["BatchFailures"] = batch.Count(result => !result.Success);

        try
        {
            renderer.Render(template, model, "../escape.txt", _root);
            useCase.Cases["Escape"] = "no-throw";
        }
        catch (InvalidOperationException error)
        {
            useCase.Cases["Escape"] = error.Message;
        }

    }

    [Then("the rendered text should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheTemplate() => ((string)useCase.Cases["RenderedText"]!).Should().Be("Hello Ada!");

    [Then("the rendered flag should be true")]
    [Trait("UAC", "02"), Fact]
    public void UAC01b_ReportsSuccess() => ((TemplateRenderResult)useCase.Cases["Rendered"]!).Success.Should().BeTrue();

    [Then("the broken failure should contain failed")]
    [Trait("UAC", "03"), Fact]
    public void UAC02_NamesTheBrokenInput() => ((TemplateRenderResult)useCase.Cases["Broken"]!).Message.Should().Contain("failed");

    [Then("the kept text should be original")]
    [Trait("UAC", "04"), Fact]
    public void UAC02b_KeepsTheExistingOutput() => ((string)useCase.Cases["KeptText"]!).Should().Be("original");

    [Then("the bad model flag should be false")]
    [Trait("UAC", "05"), Fact]
    public void UAC02c_RejectsAnInvalidModel() => ((TemplateRenderResult)useCase.Cases["BadModel"]!).Success.Should().BeFalse();

    [Then("the batch failures should be 1")]
    [Trait("UAC", "06"), Fact]
    public void UAC05_ContinuesPastAFailure() => ((int)useCase.Cases["BatchFailures"]!).Should().Be(1);

    [Then("the batch order should be one,failed,three")]
    [Trait("UAC", "07"), Fact]
    public void UAC05b_KeepsInputOrder() => ((string)useCase.Cases["BatchOrder"]!).Should().Be("one.txt,failed,three.txt");

}
