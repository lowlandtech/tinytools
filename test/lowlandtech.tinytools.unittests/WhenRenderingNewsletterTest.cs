namespace LowlandTech.TinyTools.UnitTests;

public class WhenRenderingNewsletterTest : WhenTestingFor<TinyTemplateEngine>
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

    protected override void When()
    {
        _result = Sut.Render(_template, _context);
    }

    [Fact]
    public void ItShouldRenderNewsletterHeader()
    {
        _result.Should().Contain("Tech Weekly Digest - Issue #42");
    }

    [Fact]
    public void ItShouldRenderSubscriberGreeting()
    {
        _result.Should().Contain("Hello Alex!");
    }

    [Fact]
    public void ItShouldRenderAllArticles()
    {
        _result.Should().Contain("AI Breakthroughs in 2024");
        _result.Should().Contain("Cloud Computing Trends");
        _result.Should().Contain("Cybersecurity Best Practices");
    }

    [Fact]
    public void ItShouldRenderArticleAuthors()
    {
        _result.Should().Contain("By: Dr. Smith");
        _result.Should().Contain("By: Jane Miller");
        _result.Should().Contain("By: Bob Wilson");
    }

    [Fact]
    public void ItShouldIncludeSpecialOffer()
    {
        _result.Should().Contain("? SPECIAL OFFER ?");
        _result.Should().Contain("Get 50% off our annual subscription!");
    }
}
