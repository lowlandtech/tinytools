namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingFormalLetterTest : WhenTestingFor<TinyTemplateEngine>
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
        _context = new ExecutionContext();
        _context.Set("SenderName", "Michael Chen");
        _context.Set("SenderAddress", "123 Business Ave, Suite 400");
        _context.Set("SenderCity", "New York, NY 10001");
        _context.Set("RecipientName", "Dr. Emily Watson");
        _context.Set("RecipientTitle", "Director of Research");
        _context.Set("RecipientCompany", "Innovation Labs");
        _context.Set("RecipientAddress", "456 Science Park");
        _context.Set("RecipientCity", "Boston, MA 02101");
        _context.Set("Date", "June 15, 2024");
        _context.Set("Subject", "Partnership Proposal");
        _context.Set("HasPreviousMeeting", true);
        _context.Set("MeetingDate", "last month's technology conference");

        _template = """
            ${Context.SenderName}
            ${Context.SenderAddress}
            ${Context.SenderCity}

            ${Context.Date}

            ${Context.RecipientName}
            ${Context.RecipientTitle}
            ${Context.RecipientCompany}
            ${Context.RecipientAddress}
            ${Context.RecipientCity}

            RE: ${Context.Subject}

            Dear ${Context.RecipientName},

            @if (Context.HasPreviousMeeting) {
            It was a pleasure meeting you at ${Context.MeetingDate}. I wanted to follow up on our conversation about potential collaboration opportunities.
            } else {
            I am writing to introduce myself and explore potential collaboration opportunities between our organizations.
            }

            I believe there are significant synergies between our teams that could lead to groundbreaking innovations.

            I would welcome the opportunity to discuss this further at your earliest convenience.

            Sincerely,

            ${Context.SenderName}
            """;
    }

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldRenderSenderDetails()
    {
        _result.Should().Contain("Michael Chen");
        _result.Should().Contain("123 Business Ave, Suite 400");
    }

    [Fact]
    public void ItShouldRenderRecipientDetails()
    {
        _result.Should().Contain("Dr. Emily Watson");
        _result.Should().Contain("Director of Research");
        _result.Should().Contain("Innovation Labs");
    }

    [Fact]
    public void ItShouldRenderSubjectLine()
    {
        _result.Should().Contain("RE: Partnership Proposal");
    }

    [Fact]
    public void ItShouldIncludePreviousMeetingReference()
    {
        _result.Should().Contain("It was a pleasure meeting you at last month's technology conference");
    }

    [Fact]
    public void ItShouldNotIncludeIntroductionText()
    {
        _result.Should().NotContain("I am writing to introduce myself");
    }
}
