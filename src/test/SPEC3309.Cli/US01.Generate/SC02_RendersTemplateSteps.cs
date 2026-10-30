using LowlandTech.TinyTools.Workflows;

namespace LowlandTech.TinyTools.Tests.SPEC3309.Cli.US01.Generate;

[Trait(Spec.SPEC, "3309")]
[Trait(Spec.SC, "02")]
[UserStory("01", "The generate command runs workflow template steps safely")]
public sealed class SC02_RendersTemplateSteps : IClassFixture<UseCases.GenerateUseCase>
{
    private readonly UseCases.GenerateUseCase useCase;
    private string _root = string.Empty;

    public SC02_RendersTemplateSteps(UseCases.GenerateUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    private static WorkflowDefinition Workflow(params WorkflowStep[] steps) => new() { Steps = [.. steps] };

    private static WorkflowStep Step(string output, string? forEach = null, string? item = null) => new()
    {
        Id = "tpl",
        Type = "template",
        Template = "greeting.tpl",
        Output = output,
        ForEach = forEach,
        Item = item,
    };

    [Given("a temporary workflow root with a greeting template and a three-name model")]
    private void Given()
    {
        _root = Path.Combine(Path.GetTempPath(), "tinytools-cli-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "greeting.tpl"), "Hello ${Context.Model.Name}");
    }

    [When("the template workflow runs twice, once per item and once with an escaping output")]
    private void When()
    {
        var runner = new WorkflowRunner();

        var first = runner.RunAsync(Workflow(Step("greeting.txt")), _root, new { Name = "Ada" }).GetAwaiter().GetResult();
        useCase.Cases["FirstChanged"] = first.Steps[0].Changed;
        useCase.Cases["FirstContent"] = File.ReadAllText(Path.Combine(_root, "greeting.txt"));

        var second = runner.RunAsync(Workflow(Step("greeting.txt")), _root, new { Name = "Ada" }).GetAwaiter().GetResult();
        useCase.Cases["SecondChanged"] = second.Steps[0].Changed;

        var each = runner
            .RunAsync(Workflow(Step("out-${Name}.txt", "Context.Model.Names", "Name")), _root, new { Names = new[] { "a", "b", "c" } })
            .GetAwaiter().GetResult();
        useCase.Cases["ForEachFiles"] = Directory.GetFiles(_root, "out-*.txt").Length;
        useCase.Cases["ForEachSteps"] = each.Steps.Count;

        try
        {
            runner.RunAsync(Workflow(Step("../../escape.txt")), _root, new { Name = "Ada" }).GetAwaiter().GetResult();
            useCase.Cases["EscapeMessage"] = "no-throw";
        }
        catch (InvalidOperationException error)
        {
            useCase.Cases["EscapeMessage"] = error.Message;
        }
    }

    [Then("the written template should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC04_WritesTheTemplate() => ((string)useCase.Cases["FirstContent"]!).Should().Be("Hello Ada");

    [Then("the first changed flag should be true")]
    [Trait("UAC", "02"), Fact]
    public void UAC04b_ReportsTheFirstWrite() => ((bool)useCase.Cases["FirstChanged"]!).Should().BeTrue();

    [Then("the second changed flag should be false")]
    [Trait("UAC", "03"), Fact]
    public void UAC04c_IsIdempotent() => ((bool)useCase.Cases["SecondChanged"]!).Should().BeFalse();

    [Then("the for each files should have count 3")]
    [Trait("UAC", "04"), Fact]
    public void UAC04d_WritesOneFilePerItem() => ((int)useCase.Cases["ForEachFiles"]!).Should().Be(3);

    [Then("the for each steps should have count 3")]
    [Trait("UAC", "05"), Fact]
    public void UAC04e_RunsOneStepPerItem() => ((int)useCase.Cases["ForEachSteps"]!).Should().Be(3);

    [Then("the escape message should contain escapes workflow root")]
    [Trait("UAC", "06"), Fact]
    public void UAC04f_RefusesEscapingOutputs() => ((string)useCase.Cases["EscapeMessage"]!).Should().Contain("escapes workflow root");
}
