using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "05")]
[UserStory("03", "Real-world scenarios generate practical output")]
public sealed class SC05_GeneratesNewsletter : UnitScenario
{
    private readonly TinyTemplateEngine _engine = new();
    private ExecutionContext _context = null!;
    private string _template = null!;
    private string _result = null!;

    [Given("a newsletter template with three articles and an offer")]
    protected override void Given()
    {
        _context = new ExecutionContext();
        _context.Set("NewsletterTitle", "Tech Weekly Digest");
        _context.Set("Edition", "Issue #42");
        _context.Set("SubscriberName", "Alex");
        _context.Set("Articles", new List<object>
        {
            new { Title = "AI Breakthroughs in 2024", Summary = "Latest developments in artificial intelligence.", Author = "Dr. Smith" },
            new { Title = "Cloud Computing Trends", Summary = "What to expect in cloud infrastructure.", Author = "Jane Miller" },
            new { Title = "Cybersecurity Best Practices", Summary = "Protecting your digital assets.", Author = "Bob Wilson" }
        });
        _context.Set("HasSpecialOffer", true);
        _context.Set("OfferDescription", "Get 50% off our annual subscription!");

        _template = """
            ???????????????????????????????????????
            ${Context.NewsletterTitle} - ${Context.Edition}
            ???????????????????????????????????????

            Hello ${Context.SubscriberName}!

            Here's what's new this week:

            @foreach (var article in Context.Articles) {
            ? ${article.Title}
              ${article.Summary}
              By: ${article.Author}

            }
            @if (Context.HasSpecialOffer) {
            ? SPECIAL OFFER ?
            ${Context.OfferDescription}

            }
            Thank you for reading!

            Unsubscribe: https://example.com/unsubscribe
            """;
    }

    [When("the engine renders the newsletter template")]
    protected override void When() => _result = _engine.Render(_template, _context);

    public SC05_GeneratesNewsletter() => ArrangeAndAct();

    [Then("the content should contain the newsletter header")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_RendersTheNewsletterHeader() => _result.Should().Contain("Tech Weekly Digest - Issue #42");

    [Then("the content should contain the subscriber greeting")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_RendersTheSubscriberGreeting() => _result.Should().Contain("Hello Alex!");

    [Then("the content should contain all articles")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_RendersAllArticles() => _result.Should().Contain("AI Breakthroughs in 2024").And.Contain("Cloud Computing Trends").And.Contain("Cybersecurity Best Practices");

    [Then("the content should contain the article authors")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_RendersTheArticleAuthors() => _result.Should().Contain("By: Dr. Smith").And.Contain("By: Jane Miller").And.Contain("By: Bob Wilson");

    [Then("the content should contain the special offer")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_IncludesTheSpecialOffer() => _result.Should().Contain("? SPECIAL OFFER ?").And.Contain("Get 50% off our annual subscription!");
}
