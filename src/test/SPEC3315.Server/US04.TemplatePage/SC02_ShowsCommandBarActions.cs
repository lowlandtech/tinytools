#if NET10_0_OR_GREATER
extern alias app;

using Bunit;
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Frontend.Features.Templates;
using Microsoft.Extensions.DependencyInjection;
using TemplateSessionStore = app::LowlandTech.TinyTools.App.Components.TemplateSessionStore;
using TemplatesPage = app::LowlandTech.TinyTools.App.Features.Templates.TemplatesPage;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US04.TemplatePage;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "02")]
[UserStory("04", "The app hosts the templates page on LowlandTech.Components")]
public sealed class SC02_ShowsCommandBarActions : IDisposable
{
    private readonly BunitContext _context = new();
    private IRenderedComponent<TemplatesPage> _page = null!;

    [Given("a library holding two templates")]
    private void Given()
    {
        _context.Services.AddLocalization();
        _context.Services.AddScoped<ITemplateLibrary>(_ => new TemplateLibraryStub(TemplatePageFixture.TwoTemplates()));
        _context.Services.AddScoped<TemplateEffects>();
        _context.Services.AddScoped<TemplateSessionStore>();
    }

    [When("the templates page renders")]
    private void When()
    {
        _page = _context.Render<TemplatesPage>();
        _page.WaitForState(() => _page.Markup.Contains("New template", StringComparison.Ordinal), TimeSpan.FromSeconds(5));
    }

    public SC02_ShowsCommandBarActions()
    {
        Given();
        When();
    }

    [Then("the markup should contain New template")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CommandBarActionsArePresent() => _page.Markup.Should().Contain("New template");

    public void Dispose() => _context.Dispose();
}
#endif
