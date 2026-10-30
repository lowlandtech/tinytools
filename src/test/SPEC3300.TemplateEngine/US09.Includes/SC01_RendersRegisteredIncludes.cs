namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US09.Includes;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "01")]
[UserStory("09", "Templates compose named fragments with explicit context isolation")]
public sealed class SC01_RendersRegisteredIncludes : IClassFixture<UseCases.IncludesUseCase>
{
    private readonly UseCases.IncludesUseCase useCase;

    public SC01_RendersRegisteredIncludes(UseCases.IncludesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a registered fragment set, a nested pair and a recursive pair")]
    private void Given()
    {
    }

    [When("the public Render composes each parent once")]
    private void When()
    {
        var engine = new TinyTemplateEngine();
        engine.RegisterFragment("greeting", "Hello ${Name}!");
        engine.RegisterFragment("outer", "A[@include('greeting')]B");
        engine.RegisterFragment("inner", "I[@include(\"greeting\")]J");
        engine.RegisterFragment("block", "Line one\nLine two");
        engine.RegisterFragment("empty", string.Empty);
        engine.RegisterFragment("loop-a", "a[@include('loop-b')]");
        engine.RegisterFragment("loop-b", "b[@include('loop-a')]");

        var ada = new ExecutionContext();
        ada.Set("Name", "Ada");
        var grace = new ExecutionContext();
        grace.Set("Name", "Grace");

        useCase.Cases["Included"] = engine.Render("Parent: @include('greeting')", ada);
        useCase.Cases["Nested"] = engine.Render("[@include('outer')][@include('inner')]", ada);
        useCase.Cases["CallerOne"] = engine.Render("@include('greeting')", ada);
        useCase.Cases["CallerTwo"] = engine.Render("@include('greeting')", grace);
        useCase.Cases["Multiline"] = engine.Render("@include('block')", ada);
        useCase.Cases["Empty"] = engine.Render("<@include('empty')>", ada);

        try
        {
            engine.Render("@include('absent')", ada);
            useCase.Cases["Missing"] = "no-throw";
        }
        catch (InvalidOperationException error)
        {
            useCase.Cases["Missing"] = error.Message;
        }

        try
        {
            engine.Render("@include('loop-a')", ada);
            useCase.Cases["Cycle"] = "no-throw";
        }
        catch (InvalidOperationException error)
        {
            useCase.Cases["Cycle"] = error.Message;
        }
    }

    [Then("the included text should be exact")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheNamedFragment() => ((string)useCase.Cases["Included"]!).Should().Be("Parent: Hello Ada!");

    [Then("the nested text should be exact")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersNestedFragmentsInOrder() => ((string)useCase.Cases["Nested"]!).Should().Be("[A[Hello Ada!]B][I[Hello Ada!]J]");

    [Then("the first caller text should be exact")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_UsesTheCallersOwnContext() => ((string)useCase.Cases["CallerOne"]!).Should().Be("Hello Ada!");

    [Then("the second caller text should be exact")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_DoesNotLeakTheOtherCaller() => ((string)useCase.Cases["CallerTwo"]!).Should().Be("Hello Grace!");

    [Then("the multiline text should be exact")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_KeepsFragmentLineBoundaries() => ((string)useCase.Cases["Multiline"]!).Should().Be("Line one\nLine two");

    [Then("the empty text should be exact")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RendersAnEmptyFragment() => ((string)useCase.Cases["Empty"]!).Should().Be("<>");

    [Then("the missing message should contain was not registered")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_NamesTheUnresolvedFragment() => ((string)useCase.Cases["Missing"]!).Should().Contain("Include 'absent' was not registered");

    [Then("the cycle message should contain cycle detected")]
    [Trait("UAC", "08"), Fact]
    public void UAC08_RefusesRecursiveIncludes() => ((string)useCase.Cases["Cycle"]!).Should().Contain("Include cycle detected");
}
