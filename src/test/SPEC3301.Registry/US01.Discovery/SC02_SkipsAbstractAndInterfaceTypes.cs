namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US01.Discovery;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "02")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public sealed class SC02_SkipsAbstractAndInterfaceTypes : TinyToolsScenario<TemplateRegistry>, IClassFixture<UseCases.DiscoveryUseCase>
{
    private readonly UseCases.DiscoveryUseCase useCase;

    public SC02_SkipsAbstractAndInterfaceTypes(UseCases.DiscoveryUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TemplateRegistry For() => new();

    [Given("an empty registry and an assembly that holds an abstract base and an interface")]
    protected override void Given()
    {
    }

    [When("the registry discovers the templates that the abstract base lives beside")]
    protected override void When()
    {
        base.When();

        Sut.DiscoverFromAssembly(typeof(TemplateBase).Assembly);

        useCase.Cases["Base"] = Sut.Get("Base");
        useCase.Cases["TemplateBase"] = Sut.Get("TemplateBase");
        useCase.Cases["Interface"] = Sut.Get("ITemplate");
        useCase.Cases["SingleLetter"] = Sut.Get("I");
    }

    [Then("the base registration should be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_DoesNotRegisterBaseName() => useCase.Cases["Base"].Should().BeNull();

    [Then("the template base registration should be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_DoesNotRegisterTemplateBase() => useCase.Cases["TemplateBase"].Should().BeNull();

    [Then("the interface registration should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_DoesNotRegisterInterface() => useCase.Cases["Interface"].Should().BeNull();

    [Then("the single letter registration should be null")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_DoesNotRegisterSingleLetterInterface() => useCase.Cases["SingleLetter"].Should().BeNull();
}
