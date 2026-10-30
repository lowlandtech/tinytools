using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "03")]
[UserStory("03", "Real-world scenarios generate practical output")]
public sealed class SC03_GeneratesFormalLetter : UnitScenario
{
    private readonly TinyTemplateEngine _engine = new();
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string _result = null!;

    [Given("a formal partnership letter template and its parties")]
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

    [When("the engine renders the letter template")]
    protected override void When() => _result = _engine.Render(_template, _context);

    public SC03_GeneratesFormalLetter() => ArrangeAndAct();

    [Then("the content should contain the sender details")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheSenderDetails() => _result.Should().Contain("Michael Chen").And.Contain("123 Business Ave, Suite 400");

    [Then("the content should contain the recipient details")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheRecipientDetails() => _result.Should().Contain("Dr. Emily Watson").And.Contain("Director of Research").And.Contain("Innovation Labs");

    [Then("the content should contain the subject line")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersTheSubjectLine() => _result.Should().Contain("RE: Partnership Proposal");

    [Then("the content should contain the previous meeting reference")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_IncludesThePreviousMeetingReference() => _result.Should().Contain("It was a pleasure meeting you at last month's technology conference");

    [Then("the content should not contain the introduction text")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ExcludesTheIntroductionText() => _result.Should().NotContain("I am writing to introduce myself");
}
