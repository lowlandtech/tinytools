using ExecutionContext = LowlandTech.TinyTools.ExecutionContext;

namespace LowlandTech.TinyTools.Tests.SPEC3304.Examples.US03.RealWorldScenarios;

[Trait(Spec.SPEC, "3304")]
[Trait(Spec.SC, "05")]
[UserStory("03", "Real-world scenarios generate practical output")]
public class WhenRenderingNewsletterTest : TinyToolsScenario<TinyTemplateEngine>
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

    [When("Execute test action")]
    protected override void When()
    {
        base.When();
        _result = Sut.Render(_template, _context);
    }

    [Trait(Spec.UAC, "01")]
    [Then("it Should Render Newsletter Header")]
    [Fact]
    public void ItShouldRenderNewsletterHeader()
    {
        ArrangeAndAct();
        _result.Should().Contain("Tech Weekly Digest - Issue #42");
    }

    [Trait(Spec.UAC, "02")]
    [Then("it Should Render Subscriber Greeting")]
    [Fact]
    public void ItShouldRenderSubscriberGreeting()
    {
        ArrangeAndAct();
        _result.Should().Contain("Hello Alex!");
    }

    [Trait(Spec.UAC, "03")]
    [Then("it Should Render All Articles")]
    [Fact]
    public void ItShouldRenderAllArticles()
    {
        ArrangeAndAct();
        _result.Should().Contain("AI Breakthroughs in 2024");
        _result.Should().Contain("Cloud Computing Trends");
        _result.Should().Contain("Cybersecurity Best Practices");
    }

    [Trait(Spec.UAC, "04")]
    [Then("it Should Render Article Authors")]
    [Fact]
    public void ItShouldRenderArticleAuthors()
    {
        ArrangeAndAct();
        _result.Should().Contain("By: Dr. Smith");
        _result.Should().Contain("By: Jane Miller");
        _result.Should().Contain("By: Bob Wilson");
    }

    [Trait(Spec.UAC, "05")]
    [Then("it Should Include Special Offer")]
    [Fact]
    public void ItShouldIncludeSpecialOffer()
    {
        ArrangeAndAct();
        _result.Should().Contain("? SPECIAL OFFER ?");
        _result.Should().Contain("Get 50% off our annual subscription!");
    }
}
