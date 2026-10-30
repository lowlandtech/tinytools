namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US04.ServicesAndResults;

[Trait("SPEC", "3303"), Trait("US", "04"), Trait("SC", "02")]
[UserStory("04", "Template result carries metadata")]
public sealed class SC02_CarriesMetadata : IClassFixture<UseCases.ServicesUseCase>
{
    private readonly UseCases.ServicesUseCase useCase;
    private TemplateResult result = null!;

    public SC02_CarriesMetadata(UseCases.ServicesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a rendered result carrying language, framework and version metadata")]
    private void Given()
    {
        result = new TemplateResult
        {
            Content = "content",
            Path = "test.txt",
            Namespace = "Test",
            Metadata = new Dictionary<string, object?>
            {
                ["Language"] = "TypeScript",
                ["Framework"] = "React",
                ["Version"] = "18.0"
            }
        };
    }

    [When("the metadata is read once and the result is copied with new language metadata")]
    private void When()
    {
        useCase.Cases["Metadata"] = result.Metadata;
        useCase.Cases["Language"] = result.Metadata!["Language"];
        useCase.Cases["Framework"] = result.Metadata!["Framework"];
        var modified = result with { Metadata = new Dictionary<string, object?> { ["Language"] = "C#" } };
        useCase.Cases["ModifiedLanguage"] = modified.Metadata!["Language"];
        useCase.Cases["ModifiedContent"] = modified.Content;
    }

    [Then("the metadata should not be null")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_CarriesMetadata() => useCase.Cases["Metadata"].Should().NotBeNull();

    [Then("the metadata should have count 3")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CarriesThreeEntries() => ((Dictionary<string, object?>)useCase.Cases["Metadata"]!).Should().HaveCount(3);

    [Then("the language metadata should be TypeScript")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_CarriesTheLanguage() => useCase.Cases["Language"].Should().Be("TypeScript");

    [Then("the framework metadata should be React")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_CarriesTheFramework() => useCase.Cases["Framework"].Should().Be("React");

    [Then("the copied language metadata should be C#")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_CopiesWithNewMetadata() => useCase.Cases["ModifiedLanguage"].Should().Be("C#");

    [Then("the copied content should be content")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_KeepsTheCopiedContent() => useCase.Cases["ModifiedContent"].Should().Be("content");
}
