namespace LowlandTech.TinyTools.Tests.SPEC3305.System.US01.TemplateSystem;

[Trait(Spec.SPEC, "3305")]
[Trait(Spec.SC, "06")]
[UserStory("01", "Template system validates and renders example templates")]
public sealed class SC06_NormalizesAnonymousData : TinyToolsScenario<ComponentTemplate>, IClassFixture<UseCases.TemplateSystemUseCase>
{
    private readonly UseCases.TemplateSystemUseCase useCase;
    private object _anonymousData = null!;

    public SC06_NormalizesAnonymousData(UseCases.TemplateSystemUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override ComponentTemplate For() => new();

    [Given("anonymous component data for the example template")]
    protected override void Given()
    {
        _anonymousData = new
        {
            ComponentName = "Test",
            Props = new object[] { },
            PropsDestructured = "{}"
        };
    }

    [When("the template is rendered with the anonymous data")]
    protected override void When()
    {
        base.When();
        useCase.Cases["Rendered"] = Sut.Render(_anonymousData);
    }

    [Then("the rendered template should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheAnonymousData() => useCase.Cases["Rendered"].Should().NotBeNull();

    [Then("the rendered component should contain Test")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_NormalizesTheAnonymousComponentName() => ((TemplateResult)useCase.Cases["Rendered"]!).Content.Should().Contain("Test");
}
