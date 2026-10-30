using Humanizer;

namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US04.ServicesAndResults;

[Trait("SPEC", "3303"), Trait("US", "04"), Trait("SC", "04")]
[UserStory("04", "Template services inject custom logic")]
public sealed class SC04_CalculatesThroughServices : IClassFixture<UseCases.ServicesUseCase>
{
    private readonly UseCases.ServicesUseCase useCase;
    private global::LowlandTech.TinyTools.ExecutionContext context = null!;
    private TinyTemplateEngine engine = null!;

    public SC04_CalculatesThroughServices(UseCases.ServicesUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("an empty context served by a fresh engine")]
    private void Given()
    {
        context = new global::LowlandTech.TinyTools.ExecutionContext();
        engine = new TinyTemplateEngine();
    }

    [When("a calculator service and a pluralizer each render their templates once")]
    private void When()
    {
        context.RegisterService("calc", input => new NCalc.Expression(input?.ToString() ?? "0").Evaluate());
        useCase.Cases["Simple"] = engine.Render("Result: ${Context.Services('calc')('2 + 2')}", context);
        useCase.Cases["Tax"] = engine.Render("Tax: $${Context.Services('calc')('100 * 0.15')}", context);
        context.RegisterService("pluralize", input => input?.ToString()?.Pluralize() ?? "");
        useCase.Cases["Multiple"] = engine.Render("We have ${Context.Services('calc')('5 + 3')} ${Context.Services('pluralize')('order')}.", context);
        context.RegisterService("calc", input =>
        {
            var result = new NCalc.Expression(input?.ToString() ?? "0").Evaluate();
            return result is double amount ? amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) : result;
        });
        var invoice = """
            Invoice Summary
            ---------------
            Items: ${Context.Services('calc')('5')} ${Context.Services('pluralize')('widget')}
            Subtotal: $${Context.Services('calc')('19.99 * 5')}
            Tax: $${Context.Services('calc')('19.99 * 5 * 0.08')}
            Total: $${Context.Services('calc')('19.99 * 5 * 1.08')}
            """;
        useCase.Cases["Invoice"] = engine.Render(invoice, context);
    }

    [Then("the simple expression should contain Result: 4")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_CalculatesASimpleExpression() => ((string)useCase.Cases["Simple"]!).Should().Contain("Result: 4");

    [Then("the tax expression should contain Tax: $15")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CalculatesATaxAmount() => ((string)useCase.Cases["Tax"]!).Should().Contain("Tax: $15");

    [Then("the mixed services should contain We have 8 orders.")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_CombinesTwoServices() => ((string)useCase.Cases["Multiple"]!).Should().Contain("We have 8 orders.");

    [Then("the invoice items should contain Items: 5 widgets")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersTheInvoiceItems() => ((string)useCase.Cases["Invoice"]!).Should().Contain("Items: 5 widgets");

    [Then("the invoice subtotal should contain Subtotal: $99.95")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RendersTheInvoiceSubtotal() => ((string)useCase.Cases["Invoice"]!).Should().Contain("Subtotal: $99.95");

    [Then("the invoice tax should contain Tax: $8.00")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_RendersTheInvoiceTax() => ((string)useCase.Cases["Invoice"]!).Should().Contain("Tax: $8.00");

    [Then("the invoice total should contain Total: $107.95")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_RendersTheInvoiceTotal() => ((string)useCase.Cases["Invoice"]!).Should().Contain("Total: $107.95");
}
