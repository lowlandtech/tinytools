#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "03")]
[UserStory("03", "Template effects read the server and dispatch the reducer's messages")]
public sealed class SC03_HandlesTemplateEffects : IAsyncLifetime
{
    private RecordingStore _store = null!;
    private StubLibrary _library = null!;
    private int _loaded, _cached, _selectedVersions;
    private string _order = string.Empty, _message = string.Empty, _calls = string.Empty;
    private bool _valid, _deleted;
    [Given("a library with one template that has two versions and one refusing validation")]
    private void Given()
    {
        _store = new RecordingStore();
        _library = new StubLibrary { Templates = [Template()] };
    }

    [When("load, select, validate and delete are handled once")]
    private async Task WhenAsync()
    {
        var effects = new TemplateEffects(_library);
        await effects.LoadAsync(_store);
        var loaded = (TemplatesLoaded)_store.Dispatched[0];
        _loaded = loaded.AllItems.Count;
        _cached = loaded.VersionCache.Count;

        await effects.SelectNodeAsync(_store, new TemplateSelectNode("t", "EntityClass", "template", _library.Templates[0].Id));
        var versions = (TemplateVersionsLoaded)_store.Dispatched[2];
        _selectedVersions = versions.Versions.Count;
        _order = string.Join(",", versions.Versions.Select(version => version.Name));

        await effects.ValidateVersionAsync(_store, Guid.NewGuid());
        _valid = _store.Dispatched.OfType<TemplateValidateCompleted>().Last().Result?.Valid ?? false;

        var refusing = new TemplateEffects(new StubLibrary { ValidationFailure = new InvalidOperationException("no test data") });
        await refusing.ValidateVersionAsync(_store, Guid.NewGuid());
        _message = _store.Dispatched.OfType<TemplateValidateCompleted>().Last().Error ?? string.Empty;

        await effects.DeleteAsync(_store, Guid.NewGuid());
        _deleted = _store.Dispatched.OfType<TemplateDeleteCompleted>().Any();
        _calls = string.Join(",", _library.Calls);
    }

    public async ValueTask InitializeAsync()
    {
        Given();
        await WhenAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    private static Template Template()
    {
        var id = Guid.NewGuid();
        return new Template
        {
            Id = id,
            Name = "EntityClass",
            Versions =
            [
                new TemplateVersion { Id = Guid.NewGuid(), Name = "v1", TemplateId = id },
                new TemplateVersion { Id = Guid.NewGuid(), Name = "v2", TemplateId = id },
            ],
        };
    }

    [Then("the loaded templates should have count 1")]
    [Trait("UAC", "01"), Fact]
    public void UAC01_LoadsTheLibrary() => _loaded.Should().Be(1);

    [Then("the cached versions should have count 2")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CachesEveryVersion() => _cached.Should().Be(2);

    [Then("the selection should contain v2")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_SelectsNewestFirst() => _order.Should().Be("v2,v1");

    [Then("the validation should be true")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ReportsASuccessfulValidation() => _valid.Should().BeTrue();

    [Then("the message should contain no test data")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_CarriesTheServersRefusal() => _message.Should().Contain("no test data");

    [Then("the delete should be true")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_DeletesAndReportsIt() => _deleted.Should().BeTrue();

    [Then("the calls should contain delete")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ReloadsAfterDelete() => _calls.Should().Contain("delete");
}
#endif