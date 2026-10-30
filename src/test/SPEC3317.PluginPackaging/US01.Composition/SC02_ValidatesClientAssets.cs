using LowlandTech.Plugins;

namespace LowlandTech.TinyTools.Tests.SPEC3317.PluginPackaging.US01.Composition;

[Trait(Spec.SPEC, "3317")]
[Trait(Spec.SC, "02")]
[UserStory("01", "Tinytools ships as composable Harness plugin packages")]
public sealed class SC02_ValidatesClientAssets : IClassFixture<UseCases.PluginPackagingUseCase>
{
    private const string FrontendPackage = "LowlandTech.TinyTools.Frontend";

    private readonly UseCases.PluginPackagingUseCase useCase;

    public SC02_ValidatesClientAssets(UseCases.PluginPackagingUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    private static string Verdict(string path, PluginClientAssetKind kind) =>
        PluginClientAssetPaths.TryResolve(path, FrontendPackage, kind, out _, out var reason) ? "accepted" : reason;

    [Given("the frontend package id and its declared stylesheet")]
    private void Given()
    {
    }

    [When("the declared asset and six malformed variants are resolved through the contract")]
    private void When()
    {
        var manifest = SC01_DeclaresPluginPackages.Read(Path.Combine("src", "frontend", "lowlandtech", "plugin.json"));
        var stylesheet = manifest.Assets.Stylesheets[0];
        var script = manifest.Assets.Scripts[0];

        useCase.Cases["Declared"] = stylesheet;
        useCase.Cases["Resolved"] = Verdict(stylesheet, PluginClientAssetKind.Stylesheet);
        useCase.Cases["DeclaredScript"] = script;
        useCase.Cases["ScriptResolved"] = Verdict(script, PluginClientAssetKind.Script);
        useCase.Cases["HostRelative"] = Verdict("app.css", PluginClientAssetKind.Stylesheet);
        useCase.Cases["Absolute"] = Verdict("/_content/" + FrontendPackage + "/app.css", PluginClientAssetKind.Stylesheet);
        useCase.Cases["Traversing"] = Verdict("_content/" + FrontendPackage + "/../other/app.css", PluginClientAssetKind.Stylesheet);
        useCase.Cases["Query"] = Verdict(stylesheet + "?v=1", PluginClientAssetKind.Stylesheet);
        useCase.Cases["Fragment"] = Verdict(stylesheet + "#top", PluginClientAssetKind.Stylesheet);
        useCase.Cases["WrongExtension"] = Verdict("_content/" + FrontendPackage + "/app.js", PluginClientAssetKind.Stylesheet);
    }

    [Then("the declared stylesheet should contain _content")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_DeclaresTheAssetUnderContentRoot() => ((string)useCase.Cases["Declared"]!).Should().StartWith("_content/" + FrontendPackage + "/");

    [Then("the declared script should contain templates.js")]
    [Trait("UAC", "15"), Fact]
    public void UAC07c_DeclaresTheScriptAsset() => ((string)useCase.Cases["DeclaredScript"]!).Should().Contain("templates.js");

    [Then("the declared script should be accepted")]
    [Trait("UAC", "16"), Fact]
    public void UAC07d_ResolvesTheDeclaredScript() => ((string)useCase.Cases["ScriptResolved"]!).Should().Be("accepted");

    [Then("the declared asset should be accepted")]
    [Trait("UAC", "08"), Fact]
    public void UAC07b_ResolvesTheDeclaredAsset() => ((string)useCase.Cases["Resolved"]!).Should().Be("accepted");

    [Then("the host relative path should be refused")]
    [Trait("UAC", "09"), Fact]
    public void UAC08_RefusesHostRelativePaths() => ((string)useCase.Cases["HostRelative"]!).Should().NotBe("accepted");

    [Then("the absolute path should be refused")]
    [Trait("UAC", "10"), Fact]
    public void UAC08b_RefusesAbsolutePaths() => ((string)useCase.Cases["Absolute"]!).Should().NotBe("accepted");

    [Then("the traversing path should be refused")]
    [Trait("UAC", "11"), Fact]
    public void UAC08c_RefusesTraversal() => ((string)useCase.Cases["Traversing"]!).Should().NotBe("accepted");

    [Then("the query path should be refused")]
    [Trait("UAC", "12"), Fact]
    public void UAC08d_RefusesQueryStrings() => ((string)useCase.Cases["Query"]!).Should().NotBe("accepted");

    [Then("the fragment path should be refused")]
    [Trait("UAC", "13"), Fact]
    public void UAC08e_RefusesFragments() => ((string)useCase.Cases["Fragment"]!).Should().NotBe("accepted");

    [Then("the wrong extension path should be refused")]
    [Trait("UAC", "14"), Fact]
    public void UAC08f_RefusesWrongExtensions() => ((string)useCase.Cases["WrongExtension"]!).Should().NotBe("accepted");
}
