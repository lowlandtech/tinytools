using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US04.PipeHelpers;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "04")]
[UserStory("04", "Template engine applies collection pipe helpers")]
public sealed class SC04_AppliesCollectionHelpers : TinyToolsScenario<TinyTemplateEngine>
{
    private readonly ExecutionContext _context = new();
    private string _template = string.Empty;
    private string _result = string.Empty;

    protected override TinyTemplateEngine For() => new();

    [Given("a context holding tags, numbers, an empty list and a single word")]
    protected override void Given()
    {
        _context.Set("Tags", new[] { "csharp", "dotnet", "templates" });
        _context.Set("Numbers", new[] { 1, 2, 3, 4, 5 });
        _context.Set("EmptyList", Array.Empty<string>());
        _context.Set("Word", "Hello");

        _template = """
            Count: ${Context.Tags | count}
            First: ${Context.Tags | first}
            Last: ${Context.Tags | last}
            Join: ${Context.Tags | join:, }
            Join Dash: ${Context.Numbers | join:-}
            Reverse String: ${Context.Word | reverse}
            Empty Count: ${Context.EmptyList | count}
            String Length: ${Context.Word | count}
            """;
    }

    [When("the collection helper template is rendered once")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    public SC04_AppliesCollectionHelpers() => ArrangeAndAct();

    [Then("the render should contain Count: 3")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_CountsItems() => _result.Should().Contain("Count: 3");

    [Then("the render should contain First: csharp")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_GetsFirstItem() => _result.Should().Contain("First: csharp");

    [Then("the render should contain Last: templates")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_GetsLastItem() => _result.Should().Contain("Last: templates");

    [Then("the render should contain Join: csharp, dotnet, templates")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_JoinsWithComma() => _result.Should().Contain("Join: csharp, dotnet, templates");

    [Then("the render should contain Join Dash: 1-2-3-4-5")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_JoinsWithDash() => _result.Should().Contain("Join Dash: 1-2-3-4-5");

    [Then("the render should contain Reverse String: olleH")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ReversesString() => _result.Should().Contain("Reverse String: olleH");

    [Then("the render should contain Empty Count: 0")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_CountsEmptyList() => _result.Should().Contain("Empty Count: 0");

    [Then("the render should contain String Length: 5")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_CountsStringLength() => _result.Should().Contain("String Length: 5");
}
