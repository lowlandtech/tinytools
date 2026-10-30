namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US01.Discovery;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "08")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public sealed class SC08_DiscoversFromSameAssemblyTwice : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.DiscoveryUseCase>
{
    private readonly UseCases.DiscoveryUseCase useCase;

    public SC08_DiscoversFromSameAssemblyTwice(UseCases.DiscoveryUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TemplateRegistry For() => new();

    [Given("an empty registry and the test assembly")]
    protected override void Given()
    {
    }

    [When("the same assembly is discovered twice")]
    protected override void When()
    {
        base.When();

        var assembly = typeof(ComponentTemplate).Assembly;
        Sut.DiscoverFromAssembly(assembly);
        Sut.DiscoverFromAssembly(assembly);

        useCase.Cases["Component"] = Sut.Get("Component");
        useCase.Cases["Class"] = Sut.Get("CSharpClass");
        useCase.Cases["ComponentCount"] = Sut.GetNames().Count(name => name == "Component");
    }

    [Then("the component registration should have count 1")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_DoesNotDuplicateTemplates() => useCase.Cases["ComponentCount"].Should().Be(1);

    [Then("the discovered component template should not be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_StillHasComponentTemplate() => useCase.Cases["Component"].Should().NotBeNull();

    [Then("the discovered class template should not be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_StillHasClassTemplate() => useCase.Cases["Class"].Should().NotBeNull();
}
