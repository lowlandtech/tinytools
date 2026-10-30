using LowlandTech.TinyTools.Packs;

namespace LowlandTech.TinyTools.Tests.SPEC3301.Packs.US02.Registry;

[Trait(Spec.SPEC, "3301")]
[Trait(Spec.SC, "02")]
[UserStory("02", "Template packs are admitted, enumerated and rendered as data")]
public sealed class SC02_RendersAdmittedPacks : IClassFixture<UseCases.PackRegistryUseCase>
{
    private readonly UseCases.PackRegistryUseCase useCase;

    public SC02_RendersAdmittedPacks(UseCases.PackRegistryUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    private static TemplatePackManifest Manifest(string id, string path) =>
        new(id, "1.0.0", ">=2026.1.0", [new TemplatePackEntry("greeting", path)]);

    [Given("two packs claiming the same identity and a pack carrying code-like text")]
    private void Given() { }

    [When("the second pack is admitted against the first and the code-like template is rendered")]
    private void When()
    {
        var first = TemplatePackAdmission.Admit(Manifest("same.id", "templates/greeting.tpl"), Files(), "2026.10.4", []);
        var duplicate = TemplatePackAdmission.Admit(Manifest("same.id", "templates/greeting.tpl"), Files(), "2026.10.4", [first.Templates[0].Pack]);
        useCase.Cases["Collision"] = duplicate.Refusal?.Reason;
        useCase.Cases["CollisionDetail"] = duplicate.Refusal?.Detail;

        var context = new ExecutionContext();
        context.Set("Name", "Ada");
        useCase.Cases["Rendered"] = new TinyTemplateEngine().Render(first.Templates[0].Content.Replace("${Context.Model.Name}", "${Name}"), context);

        var codePack = TemplatePackAdmission.Admit(Manifest("pack.code", "templates/greeting.tpl"), Files("<% System.IO.File.Delete(\"x\") %> ${Name}"), "2026.10.4", []);
        useCase.Cases["CodeLike"] = new TinyTemplateEngine().Render(codePack.Templates[0].Content, context);
    }

    private static Dictionary<string, string> Files(string content = "Hello ${Context.Model.Name}") =>
        new(StringComparer.OrdinalIgnoreCase) { ["templates/greeting.tpl"] = content };

    [Then("the collision reason should be duplicate-identity")]
    [Trait("UAC", "01"), Fact]
    public void UAC05_RefusesDuplicateIdentities() => ((string)useCase.Cases["Collision"]!).Should().Be("duplicate-identity");

    [Then("the collision detail should contain both identities")]
    [Trait("UAC", "02"), Fact]
    public void UAC05b_NamesBothPackages() => ((string)useCase.Cases["CollisionDetail"]!).Should().Contain("same.id@1.0.0");

    [Then("the rendered text should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC02_RendersAdmittedContent() => ((string)useCase.Cases["Rendered"]!).Should().Be("Hello Ada");

    [Then("the code like text should contain System.IO")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_TreatsCodeLikeTextAsData() => ((string)useCase.Cases["CodeLike"]!).Should().Contain("<%").And.Contain("System.IO");
}
