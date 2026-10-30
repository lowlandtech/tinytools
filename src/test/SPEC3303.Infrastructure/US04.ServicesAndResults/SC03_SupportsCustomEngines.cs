namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US04.ServicesAndResults;

[Trait("SPEC", "3303"), Trait("US", "04"), Trait("SC", "03")]
[UserStory("04", "Template services support custom engines")]
public sealed class SC03_SupportsCustomEngines : IClassFixture<UseCases.ServicesUseCase>
{
    private readonly UseCases.ServicesUseCase useCase;
    private CustomEngineTemplate template = null!;
    private SimpleData data = null!;

    public SC03_SupportsCustomEngines(UseCases.ServicesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a template constructed with an explicit engine and a named data object")]
    private void Given()
    {
        template = new CustomEngineTemplate(new TinyTemplateEngine());
        data = new SimpleData { Name = "Custom" };
    }

    [When("the template renders the data once through its custom engine")]
    private void When()
    {
        var rendered = template.Render(data);
        useCase.Cases["Result"] = rendered;
        useCase.Cases["Content"] = rendered.Content;
    }

    [Then("the rendered result should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersAResult() => useCase.Cases["Result"].Should().NotBeNull();

    [Then("the rendered content should contain Custom")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheCustomName() => ((string)useCase.Cases["Content"]!).Should().Contain("Custom");
}
