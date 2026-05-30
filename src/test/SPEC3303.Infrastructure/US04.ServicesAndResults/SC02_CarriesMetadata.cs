namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US04.ServicesAndResults;

/// <summary>
/// Tests TemplateResult metadata functionality.
/// </summary>
[Trait(Spec.SPEC, "3303")]
[Trait(Spec.SC, "02")]
[UserStory("04", "Template result carries metadata")]
public class WhenUsingTemplateResultMetadata : TinyToolsScenario<TemplateResult>
{
    private TemplateResult? _resultWithMetadata;

    protected override TemplateResult For()
    {
        return new TemplateResult
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _resultWithMetadata = Sut;
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Have Metadata")]
    [Fact]
    public void ItShouldHaveMetadata()
    {
        ArrangeAndAct();
        _resultWithMetadata!.Metadata.Should().NotBeNull();
        _resultWithMetadata.Metadata.Should().HaveCount(3);
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Contain Language Metadata")]
    [Fact]
    public void ItShouldContainLanguageMetadata()
    {
        ArrangeAndAct();
        _resultWithMetadata!.Metadata!["Language"].Should().Be("TypeScript");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Contain Framework Metadata")]
    [Fact]
    public void ItShouldContainFrameworkMetadata()
    {
        ArrangeAndAct();
        _resultWithMetadata!.Metadata!["Framework"].Should().Be("React");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Support Record With Syntax")]
    [Fact]
    public void ItShouldSupportRecordWithSyntax()
    {
        ArrangeAndAct();
        var modified = _resultWithMetadata! with
        {
            Metadata = new Dictionary<string, object?>
            {
                ["Language"] = "C#"
            }
        };

        modified.Metadata!["Language"].Should().Be("C#");
        modified.Content.Should().Be("content"); // Other properties unchanged
    }
}
