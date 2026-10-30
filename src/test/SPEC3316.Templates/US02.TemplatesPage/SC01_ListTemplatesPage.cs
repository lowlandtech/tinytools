#if NET10_0_OR_GREATER
extern alias app;

using Bunit;
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Frontend.Features.Templates;
using LowlandTech.TinyTools.Tests.SPEC3315.Server.US04.TemplatePage;
using Microsoft.Extensions.DependencyInjection;
using Page = app::LowlandTech.TinyTools.App.Features.Templates.TemplatesPage;
using TemplateSessionStore = app::LowlandTech.TinyTools.App.Components.TemplateSessionStore;

namespace LowlandTech.TinyTools.Tests.SPEC3316.Templates.US02.TemplatesPage;

[Trait(Spec.SPEC, "3316")]
[Trait(Spec.SC, "01")]
[UserStory("02", "Templates admin page displays template data")]
public sealed class SC01_ListTemplatesPage : IDisposable
{
    private readonly BunitContext _context = new();
    private IRenderedComponent<Page> _page = null!;

    [Given("a seeded template library")]
    private void Given()
    {
        _context.Services.AddLocalization();
        _context.Services.AddScoped<ITemplateLibrary>(_ => new TemplateLibraryStub(TemplatePageFixture.TwoTemplates()));
        _context.Services.AddScoped<TemplateEffects>();
        _context.Services.AddScoped<TemplateSessionStore>();
    }

    [When("the templates page is rendered")]
    private void When()
    {
        _page = _context.Render<Page>();
        _page.WaitForState(() => _page.Markup.Contains("EntityClass", StringComparison.Ordinal), TimeSpan.FromSeconds(5));
    }

    public SC01_ListTemplatesPage()
    {
        Given();
        When();
    }

    [Then("the page should contain Templates")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ContainsHeading() => _page.Markup.Should().Contain("Templates");

    [Then("the page should contain EntityClass")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_ContainsEntityClassRow() => _page.Markup.Should().Contain("EntityClass");

    public void Dispose() => _context.Dispose();
}
#endif
