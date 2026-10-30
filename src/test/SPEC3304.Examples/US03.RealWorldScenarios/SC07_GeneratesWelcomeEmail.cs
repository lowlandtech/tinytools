using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "07")]
[UserStory("03", "Real-world scenarios generate practical output")]
public sealed class SC07_GeneratesWelcomeEmail : UnitScenario, IClassFixture<UseCases.RealWorldScenariosUseCase>
{
    private readonly UseCases.RealWorldScenariosUseCase useCase;
    private readonly TinyTemplateEngine _engine = new();
    private ExecutionContext _context = null!;
    private string _template = null!;

    public SC07_GeneratesWelcomeEmail(UseCases.RealWorldScenariosUseCase useCase)
    {
        this.useCase = useCase;
        useCase.RunOnce(Given, When);
    }

    [Given("a welcome email template with a premium recipient")]
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

    [When("the engine renders the welcome email template")]
    protected override void When() => useCase.Cases["Result"] = _engine.Render(_template, _context);

    [Then("the content should contain the recipient greeting")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheRecipientName() => ((string)useCase.Cases["Result"]!).Should().Contain("Dear John Smith,");

    [Then("the content should contain the company name")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheCompanyName() => ((string)useCase.Cases["Result"]!).Should().Contain("Welcome to TechCorp Solutions!");

    [Then("the content should contain the premium message")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_IncludesThePremiumMessage() => ((string)useCase.Cases["Result"]!).Should().Contain("As a Premium member");

    [Then("the content should contain the sender signature")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersTheSenderSignature() => ((string)useCase.Cases["Result"]!).Should().Contain("Sarah Johnson").And.Contain("Customer Success Manager");
}
