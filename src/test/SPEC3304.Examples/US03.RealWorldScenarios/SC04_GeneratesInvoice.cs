namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "04")]
[UserStory("03", "Real-world scenarios generate practical output")]
public class WhenRenderingInvoiceTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ToolContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("Setup test context")]
    protected override void Given()
    {
        _context = new ToolContext();
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Render Invoice Header")]
    [Fact]
    public void ItShouldRenderInvoiceHeader()
    {
        ArrangeAndAct();
        _result.Should().Contain("INVOICE");
        _result.Should().Contain("Invoice #: INV-2024-0042");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Render Company Details")]
    [Fact]
    public void ItShouldRenderCompanyDetails()
    {
        ArrangeAndAct();
        _result.Should().Contain("TechServices LLC");
        _result.Should().Contain("789 Commerce Street, Chicago, IL 60601");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Render Client Details")]
    [Fact]
    public void ItShouldRenderClientDetails()
    {
        ArrangeAndAct();
        _result.Should().Contain("Global Enterprises");
        _result.Should().Contain("Robert Johnson");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Render All Line Items")]
    [Fact]
    public void ItShouldRenderAllLineItems()
    {
        ArrangeAndAct();
        _result.Should().Contain("Web Development Services");
        _result.Should().Contain("UI/UX Design");
        _result.Should().Contain("Project Management");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Render Totals")]
    [Fact]
    public void ItShouldRenderTotals()
    {
        ArrangeAndAct();
        _result.Should().Contain("Subtotal:    $9500");
        _result.Should().Contain("TOTAL:       $10260");
    }

    [Trait(Spec.UAC, "06")]
    [Then("it Should Show Payment Due Message")]
    [Fact]
    public void ItShouldShowPaymentDueMessage()
    {
        ArrangeAndAct();
        _result.Should().Contain("Payment due by July 15, 2024");
        _result.Should().NotContain("? PAID");
    }
}
