using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "09")]
[UserStory("03", "Real-world scenarios generate practical output")]
public sealed class SC09_GeneratesInvoiceItemsAndTotals : UnitScenario
{
    private readonly TinyTemplateEngine _engine = new();
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string _result = null!;

    [Given("an invoice template with billing and client data")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("InvoiceNumber", "INV-2024-0042");
        _context.Set("InvoiceDate", "June 15, 2024");
        _context.Set("DueDate", "July 15, 2024");
        _context.Set("CompanyName", "TechServices LLC");
        _context.Set("CompanyAddress", "789 Commerce Street, Chicago, IL 60601");
        _context.Set("ClientName", "Global Enterprises");
        _context.Set("ClientContact", "Robert Johnson");
        _context.Set("ClientAddress", "321 Corporate Blvd, Dallas, TX 75201");
        _context.Set("LineItems", new List<object>
        {
            new { Description = "Web Development Services", Hours = 40, Rate = "150.00", Amount = "6000.00" },
            new { Description = "UI/UX Design", Hours = 20, Rate = "125.00", Amount = "2500.00" },
            new { Description = "Project Management", Hours = 10, Rate = "100.00", Amount = "1000.00" }
        });
        _context.Set("Subtotal", "9500.00");
        _context.Set("TaxRate", "8%");
        _context.Set("TaxAmount", "760.00");
        _context.Set("Total", "10260.00");
        _context.Set("IsPaid", false);
        _context.Set("PaymentTerms", "Net 30");

        _template = """
            ????????????????????????????????????????????????????????????
            ?                        INVOICE                           ?
            ????????????????????????????????????????????????????????????

            From: ${Context.CompanyName}
                  ${Context.CompanyAddress}

            To:   ${Context.ClientName}
                  Attn: ${Context.ClientContact}
                  ${Context.ClientAddress}

            Invoice #: ${Context.InvoiceNumber}
            Date:      ${Context.InvoiceDate}
            Due Date:  ${Context.DueDate}
            Terms:     ${Context.PaymentTerms}

            ??????????????????????????????????????????????????????????????
            Description                    Hours    Rate      Amount
            ??????????????????????????????????????????????????????????????
            @foreach (var item in Context.LineItems) {
            ${item.Description}    ${item.Hours}    $${item.Rate}    $${item.Amount}
            }
            ??????????????????????????????????????????????????????????????
                                               Subtotal:    $${Context.Subtotal}
                                               Tax (${Context.TaxRate}):   $${Context.TaxAmount}
                                               ?????????????????????????
                                               TOTAL:       $${Context.Total}

            @if (Context.IsPaid) {
            ? PAID - Thank you for your payment!
            } else {
            Payment due by ${Context.DueDate}. Please remit payment to the address above.
            }

            Thank you for your business!
            """;
    }

    [When("the engine renders the invoice template")]
    protected override void When() => _result = _engine.Render(_template, _context);

    public SC09_GeneratesInvoiceItemsAndTotals() => ArrangeAndAct();

    [Then("the content should contain all line items")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersAllLineItems() => _result.Should().Contain("Web Development Services").And.Contain("UI/UX Design").And.Contain("Project Management");

    [Then("the content should contain the totals")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_RendersTheTotals() => _result.Should().Contain("Subtotal:    $9500").And.Contain("TOTAL:       $10260");

    [Then("the content should contain the payment due message")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_ShowsThePaymentDueMessage() => _result.Should().Contain("Payment due by July 15, 2024").And.NotContain("? PAID");
}
