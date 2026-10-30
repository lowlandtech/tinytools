using System.Globalization;
using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US04.PipeHelpers;

[Trait(Spec.SPEC, "3300")]
[Trait(Spec.SC, "02")]
[UserStory("04", "Template engine applies number pipe helpers")]
public sealed class SC02_AppliesNumberHelpers : TinyToolsScenario<TinyTemplateEngine>, IClassFixture<UseCases.PipeHelpersUseCase>
{
    private readonly UseCases.PipeHelpersUseCase useCase;
    private readonly ExecutionContext _context = new();
    private string _template = string.Empty;

    public SC02_AppliesNumberHelpers(UseCases.PipeHelpersUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    protected override TinyTemplateEngine For() => new();

    [Given("a context holding a price, a quantity, a percentage and pi under the invariant culture")]
    protected override void Given()
    {
        // Set invariant culture for consistent test results
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        _context.Set("Price", 1234.567);
        _context.Set("Quantity", 1500);
        _context.Set("Percentage", 0.856);
        _context.Set("Pi", 3.14159265);

        _template = """
            Formatted Number: ${Context.Quantity | number}
            Two Decimals: ${Context.Price | format:N2}
            Percentage: ${Context.Percentage | format:P0}
            Round 2: ${Context.Pi | round:2}
            Floor: ${Context.Price | floor}
            Ceiling: ${Context.Price | ceiling}
            """;
    }

    [When("the number helper template is rendered once")]
    protected override void When()
    {
        base.When();
        useCase.Cases["Rendered"] = Sut.Render(_template, _context);
    }

    [Then("the render should contain Formatted Number: 1,500")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_FormatsNumber() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Formatted Number: 1,500");

    [Then("the render should contain Two Decimals: 1,234.57")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_FormatsWithTwoDecimals() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Two Decimals: 1,234.57");

    [Then("the percentage render should contain 86 and a percent sign")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_FormatsAsPercentage() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Percentage: 86").And.Contain("%");

    [Then("the render should contain Round 2: 3.14")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RoundsToTwoDecimals() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Round 2: 3.14");

    [Then("the render should contain Floor: 1234")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_AppliesFloor() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Floor: 1234");

    [Then("the render should contain Ceiling: 1235")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_AppliesCeiling() => ((string)useCase.Cases["Rendered"]!).Should().Contain("Ceiling: 1235");
}
