using System.Globalization;

namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US05.InvariantInterpolation;

[Trait(Spec.SPEC, "3303")]
[Trait(Spec.SC, "01")]
[UserStory("05", "Interpolation renders the same text on every machine")]
public sealed class SC01_RendersInvariantText : IClassFixture<UseCases.InvariantInterpolationUseCase>
{
    private readonly UseCases.InvariantInterpolationUseCase useCase;
    private readonly TinyTemplateEngine engine = new();
    private ExecutionContext context = null!;

    public SC01_RendersInvariantText(UseCases.InvariantInterpolationUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a caller context with numbers, a string, a boolean and a null value")]
    private void Given()
    {
        context = new ExecutionContext();
        context.Set("Value", 1234.5);
        context.Set("Ratio", 3.14159);
        context.Set("Name", "Ada");
        context.Set("Flag", true);
    }

    [When("each template renders under a comma-decimal culture")]
    private void When()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("nl-NL");
        try
        {
            useCase.Cases["Number"] = engine.Render("${Context.Value}", context);
            useCase.Cases["Rounded"] = engine.Render("${Context.Ratio | round:2}", context);
            useCase.Cases["Formatted"] = engine.Render("${Context.Ratio | format:0.00}", context);
            useCase.Cases["Boolean"] = engine.Render("${Context.Flag}", context);
            useCase.Cases["String"] = engine.Render("${Context.Name}", context);
            useCase.Cases["Null"] = engine.Render("[${Context.Absent}]", context);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Then("the number text should be invariant")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheNumberInvariantly() => ((string)useCase.Cases["Number"]!).Should().Be("1234.5");

    [Then("the rounded text should be invariant")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheRoundedValueInvariantly() => ((string)useCase.Cases["Rounded"]!).Should().Be("3.14");

    [Then("the formatted text should be invariant")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_AgreesWithTheFormatHelper() => ((string)useCase.Cases["Formatted"]!).Should().Be("3.14");

    [Then("the boolean text should be True")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_KeepsBooleanText() => ((string)useCase.Cases["Boolean"]!).Should().Be("True");

    [Then("the string text should be Ada")]
    [Trait("UAC", "05"), Fact]
    public void UAC04b_KeepsStringText() => ((string)useCase.Cases["String"]!).Should().Be("Ada");

    [Then("the null text should be empty")]
    [Trait("UAC", "06"), Fact]
    public void UAC04c_KeepsNullText() => ((string)useCase.Cases["Null"]!).Should().Be("[]");
}
