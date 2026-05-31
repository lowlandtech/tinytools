using ExecutionContext = LowlandTech.TinyTools.Core.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US04.PipeHelpers;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "04")]
[UserStory("04", "Template engine applies collection pipe helpers")]
public class WhenRenderingWithCollectionHelpersTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _context = new ExecutionContext();
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Count Items")]
    [Fact]
    public void ItShouldCountItems()
    {
        ArrangeAndAct();
        _result.Should().Contain("Count: 3");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Get First Item")]
    [Fact]
    public void ItShouldGetFirstItem()
    {
        ArrangeAndAct();
        _result.Should().Contain("First: csharp");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Get Last Item")]
    [Fact]
    public void ItShouldGetLastItem()
    {
        ArrangeAndAct();
        _result.Should().Contain("Last: templates");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Join With Comma")]
    [Fact]
    public void ItShouldJoinWithComma()
    {
        ArrangeAndAct();
        _result.Should().Contain("Join: csharp, dotnet, templates");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Join With Dash")]
    [Fact]
    public void ItShouldJoinWithDash()
    {
        ArrangeAndAct();
        _result.Should().Contain("Join Dash: 1-2-3-4-5");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Reverse String")]
    [Fact]
    public void ItShouldReverseString()
    {
        ArrangeAndAct();
        _result.Should().Contain("Reverse String: olleH");
    }

    [Trait(Spec.UAC, "07")]
    [Then("it Should Count Empty List")]
    [Fact]
    public void ItShouldCountEmptyList()
    {
        ArrangeAndAct();
        _result.Should().Contain("Empty Count: 0");
    }

    [Trait(Spec.UAC, "08")]
    [Then("it Should Count String Length")]
    [Fact]
    public void ItShouldCountStringLength()
    {
        ArrangeAndAct();
        _result.Should().Contain("String Length: 5");
    }
}
