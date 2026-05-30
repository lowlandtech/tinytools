namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "03")]
[UserStory("03", "Real-world scenarios generate practical output")]
public class WhenRenderingFormalLetterTest : TinyToolsScenario<TinyTemplateEngine>
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Render Sender Details")]
    [Fact]
    public void ItShouldRenderSenderDetails()
    {
        ArrangeAndAct();
        _result.Should().Contain("Michael Chen");
        _result.Should().Contain("123 Business Ave, Suite 400");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Render Recipient Details")]
    [Fact]
    public void ItShouldRenderRecipientDetails()
    {
        ArrangeAndAct();
        _result.Should().Contain("Dr. Emily Watson");
        _result.Should().Contain("Director of Research");
        _result.Should().Contain("Innovation Labs");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Render Subject Line")]
    [Fact]
    public void ItShouldRenderSubjectLine()
    {
        ArrangeAndAct();
        _result.Should().Contain("RE: Partnership Proposal");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Include Previous Meeting Reference")]
    [Fact]
    public void ItShouldIncludePreviousMeetingReference()
    {
        ArrangeAndAct();
        _result.Should().Contain("It was a pleasure meeting you at last month's technology conference");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Not Include Introduction Text")]
    [Fact]
    public void ItShouldNotIncludeIntroductionText()
    {
        ArrangeAndAct();
        _result.Should().NotContain("I am writing to introduce myself");
    }
}
