using LowlandTech.TinyTools.Packs;

namespace LowlandTech.TinyTools.Tests.SPEC3301.Packs.US02.Registry;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Template packs are admitted, enumerated and rendered as data")]
public sealed class SC01_AdmitsTemplatePacks : IClassFixture<UseCases.PackRegistryUseCase>
{
    private readonly UseCases.PackRegistryUseCase useCase;

    public SC01_AdmitsTemplatePacks(UseCases.PackRegistryUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    private static TemplatePackManifest Manifest(string id, string engine = ">=2026.1.0") => new(
        id,
        "1.0.0",
        engine,
        [new TemplatePackEntry("greeting", "templates/greeting.tpl")]);

    private static Dictionary<string, string> Files(string content = "Hello ${Context.Model.Name}") =>
        new(StringComparer.OrdinalIgnoreCase) { ["templates/greeting.tpl"] = content };

    [Given("a valid pack, an incompatible one, malformed packs and a colliding pair")]
    private void Given() { }

    [When("each pack is admitted against the engine and the admitted set")]
    private void When()
    {
        var valid = TemplatePackAdmission.Admit(Manifest("pack.one"), Files(), "2026.10.4", []);
        useCase.Cases["ValidAdmitted"] = valid.Admitted;
        useCase.Cases["Identity"] = valid.Templates[0].Pack.Identity;
        useCase.Cases["IdentityStable"] = valid.Templates[0].Pack.Identity == Manifest("pack.one").Identity;
        useCase.Cases["TemplateName"] = valid.Templates[0].Name;
        useCase.Cases["Provenance"] = valid.Templates[0].Pack.Id;

        var incompatible = TemplatePackAdmission.Admit(Manifest("pack.two", "[2027.1,2028.0)"), Files(), "2026.10.4", []);
        useCase.Cases["Incompatible"] = incompatible.Refusal?.Reason;

        var compatible = TemplatePackAdmission.Admit(Manifest("pack.three", "[2026.1,2027.0)"), Files(), "2026.10.4", []);
        useCase.Cases["CompatibleAdmitted"] = compatible.Admitted;

        var traversalPack = new TemplatePackManifest(
            "pack.four", "1.0.0", ">=2026.1.0", [new TemplatePackEntry("escape", "../escape.tpl")]);
        useCase.Cases["Traversal"] = TemplatePackAdmission
            .Admit(traversalPack, new Dictionary<string, string> { ["../escape.tpl"] = "x" }, "2026.10.4", []).Refusal?.Reason;
        useCase.Cases["Executable"] = TemplatePackAdmission
            .Admit(Manifest("pack.five"), new Dictionary<string, string> { ["templates/greeting.tpl"] = "x", ["evil.ps1"] = "x" }, "2026.10.4", []).Refusal?.Reason;
        useCase.Cases["Undeclared"] = TemplatePackAdmission
            .Admit(Manifest("pack.six"), new Dictionary<string, string> { ["templates/greeting.tpl"] = "x", ["templates/other.tpl"] = "x" }, "2026.10.4", []).Refusal?.Reason;
    }

    [Then("the pack admitted flag should be true")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_AdmitsAValidPack() => ((bool)useCase.Cases["ValidAdmitted"]!).Should().BeTrue();

    [Then("the identity should be stable")]
    [Trait("UAC", "02"), Fact]
    public void UAC01b_KeepsAStableIdentity() => ((bool)useCase.Cases["IdentityStable"]!).Should().BeTrue();

    [Then("the identity should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC01c_CombinesIdAndVersion() => ((string)useCase.Cases["Identity"]!).Should().Be("pack.one@1.0.0");

    [Then("the incompatible reason should be engine-incompatible")]
    [Trait("UAC", "04"), Fact]
    public void UAC01d_RejectsAnIncompatibleEngine() => ((string)useCase.Cases["Incompatible"]!).Should().Be("engine-incompatible");

    [Then("the compatible admitted flag should be true")]
    [Trait("UAC", "05"), Fact]
    public void UAC01e_AdmitsAnInRangeEngine() => ((bool)useCase.Cases["CompatibleAdmitted"]!).Should().BeTrue();

    [Then("the template name should be greeting")]
    [Trait("UAC", "06"), Fact]
    public void UAC02_EnumeratesDeclaredTemplates() => ((string)useCase.Cases["TemplateName"]!).Should().Be("greeting");

    [Then("the provenance should be pack.one")]
    [Trait("UAC", "07"), Fact]
    public void UAC02b_CarriesPackProvenance() => ((string)useCase.Cases["Provenance"]!).Should().Be("pack.one");

    [Then("the traversal reason should be path-traversal")]
    [Trait("UAC", "08"), Fact]
    public void UAC03_RefusesTraversal() => ((string)useCase.Cases["Traversal"]!).Should().Be("path-traversal");

    [Then("the executable reason should be undeclared-file")]
    [Trait("UAC", "09"), Fact]
    public void UAC03b_RefusesExecutableContent() => ((string)useCase.Cases["Executable"]!).Should().Be("undeclared-file");

    [Then("the undeclared reason should be undeclared-file")]
    [Trait("UAC", "10"), Fact]
    public void UAC03c_RefusesUndeclaredFiles() => ((string)useCase.Cases["Undeclared"]!).Should().Be("undeclared-file");
}
