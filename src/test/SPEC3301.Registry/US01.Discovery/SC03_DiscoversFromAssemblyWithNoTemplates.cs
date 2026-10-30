namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US01.Discovery;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "03")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public sealed class SC03_DiscoversFromAssemblyWithNoTemplates : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.DiscoveryUseCase>
{
    private readonly UseCases.DiscoveryUseCase useCase;

    public SC03_DiscoversFromAssemblyWithNoTemplates(UseCases.DiscoveryUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TemplateRegistry For() => new();

    [Given("an empty registry and an assembly that holds no template")]
    protected override void Given()
    {
    }

    [When("the registry discovers the templates of the system assembly")]
    protected override void When()
    {
        base.When();

        Sut.DiscoverFromAssembly(typeof(string).Assembly);

        useCase.Cases["Names"] = Sut.GetNames().ToList();
        useCase.Cases["All"] = Sut.GetAll().ToList();
    }

    [Then("the discovered names should have count 0")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_DiscoversNoTemplates() => ((List<string>)useCase.Cases["Names"]!).Should().BeEmpty();

    [Then("the discovered templates should have count 0")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_DoesNotThrow() => ((List<ITemplate>)useCase.Cases["All"]!).Should().BeEmpty();
}
