#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "03")]
[UserStory("03", "Template effects read the server and dispatch the reducer's messages")]
public sealed class SC03_HandlesTemplateEffects : IAsyncLifetime, IClassFixture<UseCases.TemplateStoreUseCase>
{
    private readonly UseCases.TemplateStoreUseCase useCase;
    private RecordingStore _store = null!;
    private StubLibrary _library = null!;

    public SC03_HandlesTemplateEffects(UseCases.TemplateStoreUseCase useCase) => this.useCase = useCase;

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
        useCase.Cases["Loaded"] = loaded.AllItems.Count;
        useCase.Cases["Cached"] = loaded.VersionCache.Count;

        await effects.SelectNodeAsync(_store, new TemplateSelectNode("t", "EntityClass", "template", _library.Templates[0].Id));
        var versions = (TemplateVersionsLoaded)_store.Dispatched[2];
        useCase.Cases["Order"] = string.Join(",", versions.Versions.Select(version => version.Name));

        await effects.ValidateVersionAsync(_store, Guid.NewGuid());
        useCase.Cases["Valid"] = _store.Dispatched.OfType<TemplateValidateCompleted>().Last().Result?.Valid ?? false;

        var refusing = new TemplateEffects(new StubLibrary { ValidationFailure = new InvalidOperationException("no test data") });
        await refusing.ValidateVersionAsync(_store, Guid.NewGuid());
        useCase.Cases["Message"] = _store.Dispatched.OfType<TemplateValidateCompleted>().Last().Error ?? string.Empty;

        await effects.DeleteAsync(_store, Guid.NewGuid());
        useCase.Cases["Deleted"] = _store.Dispatched.OfType<TemplateDeleteCompleted>().Any();
        useCase.Cases["Calls"] = string.Join(",", _library.Calls);
    }

    public ValueTask InitializeAsync()
    {
        useCase.RunOnce(Given, () => WhenAsync().GetAwaiter().GetResult());
        return ValueTask.CompletedTask;
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
    public void UAC01_LoadsTheLibrary() => ((int)useCase.Cases["Loaded"]!).Should().Be(1);

    [Then("the cached versions should have count 2")]
    [Trait("UAC", "02"), Fact]
    public void UAC02_CachesEveryVersion() => ((int)useCase.Cases["Cached"]!).Should().Be(2);

    [Then("the selection should contain v2")]
    [Trait("UAC", "03"), Fact]
    public void UAC03_SelectsNewestFirst() => ((string)useCase.Cases["Order"]!).Should().Be("v2,v1");

    [Then("the validation should be true")]
    [Trait("UAC", "04"), Fact]
    public void UAC04_ReportsASuccessfulValidation() => ((bool)useCase.Cases["Valid"]!).Should().Be(true);

    [Then("the message should contain no test data")]
    [Trait("UAC", "05"), Fact]
    public void UAC05_CarriesTheServersRefusal() => ((string)useCase.Cases["Message"]!).Should().Contain("no test data");

    [Then("the delete should be true")]
    [Trait("UAC", "06"), Fact]
    public void UAC06_DeletesAndReportsIt() => ((bool)useCase.Cases["Deleted"]!).Should().Be(true);

    [Then("the calls should contain delete")]
    [Trait("UAC", "07"), Fact]
    public void UAC07_ReloadsAfterDelete() => ((string)useCase.Cases["Calls"]!).Should().Contain("delete");
}
#endif
