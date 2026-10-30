#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Frontend.Services.Http;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "02")]
[UserStory("03", "The templates store reaches the server through one HTTP client")]
public sealed class SC02_CallsTheTemplateApi : IAsyncLifetime
{
    private RoutedTemplateHandler _handler = null!;
    private HttpTemplateLibrary _library = null!;
    private int _listed;
    private string _created = string.Empty, _message = string.Empty, _calls = string.Empty;
    private bool _missingIsNull, _deleted, _valid;

    [Given("a routed template API behind one HTTP client")]
    private void Given()
    {
        _handler = new RoutedTemplateHandler();
        var client = new HttpClient(_handler) { BaseAddress = new Uri("http://tinytools.test") };
        _library = new HttpTemplateLibrary(client);
    }

    [When("list, create, get-missing, delete, validate and the refusal are called")]
    private async Task WhenAsync()
    {
        _listed = (await _library.ListAsync()).Count;
        _created = (await _library.CreateAsync(new Template { Name = "EntityClass" })).Name;
        _missingIsNull = await _library.GetAsync(Guid.NewGuid()) is null;
        _deleted = await _library.DeleteAsync(Guid.NewGuid());
        _valid = (await _library.ValidateVersionAsync(Guid.NewGuid()))!.Valid;
        try { await _library.ValidateVersionAsync(RoutedTemplateHandler.RefusingVersion); }
        catch (InvalidOperationException exception) { _message = exception.Message; }
        _calls = string.Join(" | ", _handler.Calls);
    }

    public async ValueTask InitializeAsync()
    {
        Given();
        await WhenAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Then("the listed templates should have count 1")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_ListsThroughTheDocumentedRoute() => _listed.Should().Be(1);

    [Then("the created name should contain EntityClass")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CreatesThroughTheRoute() => _created.Should().Contain("EntityClass");

    [Then("the missing template should be null")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_ReportsMissingAsNull() => _missingIsNull.Should().BeTrue();

    [Then("the delete should be true")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_DeletesThroughTheRoute() => _deleted.Should().BeTrue();

    [Then("the validation should be true")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_ValidatesAVersion() => _valid.Should().BeTrue();

    [Then("the refusal message should contain no test data")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_SurfacesTheServersRefusal() => _message.Should().Contain("no test data");

    [Then("the calls should contain delete")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_UsesTheDocumentedMethods() => _calls.Should().Contain("DELETE");
}
#endif