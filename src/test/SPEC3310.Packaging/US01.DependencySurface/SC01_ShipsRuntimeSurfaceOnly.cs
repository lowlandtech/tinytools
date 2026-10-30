namespace LowlandTech.TinyTools.Tests.SPEC3310.Packaging.US01.DependencySurface;

[Trait(Spec.SPEC, "3310")]
[Trait(Spec.SC, "01")]
[UserStory("01", "The shipped package exposes only runtime surface")]
public sealed class SC01_ShipsRuntimeSurfaceOnly : IClassFixture<UseCases.DependencySurfaceUseCase>
{
    private readonly UseCases.DependencySurfaceUseCase useCase;

    public SC01_ShipsRuntimeSurfaceOnly(UseCases.DependencySurfaceUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("the shipped library assembly")]
    private void Given()
    {
    }

    [When("its public types and referenced assemblies are enumerated")]
    private void When()
    {
        var assembly = typeof(TinyTemplateEngine).Assembly;
        useCase.Cases["PublicTypes"] = assembly.GetExportedTypes().Select(type => type.Name).ToList();
        useCase.Cases["References"] = assembly.GetReferencedAssemblies().Select(name => name.Name ?? string.Empty).ToList();
    }

    [Then("the test logger helper should be absent")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_OmitsTheMockingLoggerHelper() => ((List<string>)useCase.Cases["PublicTypes"]!).Should().NotContain("LoggerExtensions");

    [Then("the legacy test base should be absent")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_OmitsTheLegacyTestBase() => ((List<string>)useCase.Cases["PublicTypes"]!).Should().NotContain("WhenTestingFor`1");

    [Then("the moq reference should be absent")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_DoesNotReferenceMoq() => ((List<string>)useCase.Cases["References"]!).Should().NotContain("Moq");

    [Then("the shipped public types should not contain test scaffolding")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ExportsOnlyRuntimeTypes() => ((List<string>)useCase.Cases["PublicTypes"]!).Should().NotContain(name => name.Contains("Test", StringComparison.Ordinal) && name != "TemplateTestData");
}
