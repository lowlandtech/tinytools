using FluentAssertions;

namespace LowlandTech.TinyTools.UnitTests;

/// <summary>
/// Tests TemplateResult metadata functionality.
/// </summary>
public class WhenUsingTemplateResultMetadata : WhenTestingFor<TemplateResult>
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

    protected override void When()
    {
        _resultWithMetadata = Sut;
    }

    [Fact]
    public void ItShouldHaveMetadata()
    {
        _resultWithMetadata!.Metadata.Should().NotBeNull();
        _resultWithMetadata.Metadata.Should().HaveCount(3);
    }

    [Fact]
    public void ItShouldContainLanguageMetadata()
    {
        _resultWithMetadata!.Metadata!["Language"].Should().Be("TypeScript");
    }

    [Fact]
    public void ItShouldContainFrameworkMetadata()
    {
        _resultWithMetadata!.Metadata!["Framework"].Should().Be("React");
    }

    [Fact]
    public void ItShouldSupportRecordWithSyntax()
    {
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
