namespace LowlandTech.TinyTools.Tests.SPEC3301.Registry.US01.Discovery;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "02")]
[UserStory("01", "Template registry discovers templates from assemblies")]
public sealed class SC02_SkipsAbstractAndInterfaceTypes : TinyToolsScenario<TemplateRegistry>
{
    private ITemplate? _base;
    private ITemplate? _templateBase;
    private ITemplate? _interface;
    private ITemplate? _singleLetter;

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

        _base = Sut.Get("Base");
        _templateBase = Sut.Get("TemplateBase");
        _interface = Sut.Get("ITemplate");
        _singleLetter = Sut.Get("I");
    }

    public SC02_SkipsAbstractAndInterfaceTypes() => ArrangeAndAct();

    [Then("the base registration should be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_DoesNotRegisterBaseName() => _base.Should().BeNull();

    [Then("the template base registration should be null")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_DoesNotRegisterTemplateBase() => _templateBase.Should().BeNull();

    [Then("the interface registration should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_DoesNotRegisterInterface() => _interface.Should().BeNull();

    [Then("the single letter registration should be null")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_DoesNotRegisterSingleLetterInterface() => _singleLetter.Should().BeNull();
}
