using ExecutionContext = LowlandTech.TinyTools.Core.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "07")]
[UserStory("03", "Real-world scenarios generate practical output")]
public class WhenRenderingWelcomeEmailTest : TinyToolsScenario<TinyTemplateEngine>
{
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string? _result;

    protected override TinyTemplateEngine For()
    {
        return new TinyTemplateEngine();
    }

    [Given("Setup test context")]
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Render Recipient Name")]
    [Fact]
    public void ItShouldRenderRecipientName()
    {
        ArrangeAndAct();
        _result.Should().Contain("Dear John Smith,");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Render Company Name")]
    [Fact]
    public void ItShouldRenderCompanyName()
    {
        ArrangeAndAct();
        _result.Should().Contain("Welcome to TechCorp Solutions!");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Include Premium Message")]
    [Fact]
    public void ItShouldIncludePremiumMessage()
    {
        ArrangeAndAct();
        _result.Should().Contain("As a Premium member");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Render Sender Signature")]
    [Fact]
    public void ItShouldRenderSenderSignature()
    {
        ArrangeAndAct();
        _result.Should().Contain("Sarah Johnson");
        _result.Should().Contain("Customer Success Manager");
    }
}
