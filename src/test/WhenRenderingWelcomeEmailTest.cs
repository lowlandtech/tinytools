namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingWelcomeEmailTest : WhenTestingFor<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    protected override void Given()
    {
        _context = new ExecutionContext
        {
            Model = new EmailContext
            {
                SenderName = "Sarah Johnson",
                SenderTitle = "Customer Success Manager",
                CompanyName = "TechCorp Solutions",
                Recipient = new Customer
                {
                    FirstName = "John",
                    LastName = "Smith",
                    Company = "Acme Inc",
                    IsPremium = true
                }
            }
        };

        _template = """
            Dear ${Context.Model.Recipient.FirstName} ${Context.Model.Recipient.LastName},

            Welcome to ${Context.Model.CompanyName}! We're thrilled to have you on board.

            @if (Context.Model.Recipient.IsPremium) {
            As a Premium member, you have access to our exclusive features and priority support.
            }

            If you have any questions, please don't hesitate to reach out.

            Best regards,
            ${Context.Model.SenderName}
            ${Context.Model.SenderTitle}
            """;
    }

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldRenderRecipientName()
    {
        _result.Should().Contain("Dear John Smith,");
    }

    [Fact]
    public void ItShouldRenderCompanyName()
    {
        _result.Should().Contain("Welcome to TechCorp Solutions!");
    }

    [Fact]
    public void ItShouldIncludePremiumMessage()
    {
        _result.Should().Contain("As a Premium member");
    }

    [Fact]
    public void ItShouldRenderSenderSignature()
    {
        _result.Should().Contain("Sarah Johnson");
        _result.Should().Contain("Customer Success Manager");
    }
}
