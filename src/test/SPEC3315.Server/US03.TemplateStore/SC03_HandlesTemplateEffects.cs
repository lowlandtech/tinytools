#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

/// <summary>A store that records what the effects dispatch.</summary>
public sealed class RecordingStore(TemplateState? state = null) : ITemplateStore
{
    public TemplateState State { get; } = state ?? new TemplateState();
    public List<object> Dispatched { get; } = [];

    public void Dispatch(object action) => Dispatched.Add(action);
}

/// <summary>A library that answers with fixed data and records the calls.</summary>
public sealed class StubLibrary : ITemplateLibrary
{
    public List<Template> Templates { get; init; } = [];
    public TemplateValidation? Validation { get; init; } = new(true, "out", "out");
    public Exception? ValidationFailure { get; init; }
    public List<(string Call, Guid? Id)> Calls { get; } = [];

    public Task<List<Template>> ListAsync(CancellationToken ct = default)
    {
        Calls.Add(("list", null));
        return Task.FromResult(Templates);
    }

    public Task<Template?> GetAsync(Guid id, CancellationToken ct = default)
    {
        Calls.Add(("get", id));
        return Task.FromResult(Templates.FirstOrDefault(template => template.Id == id));
    }

    public Task<Template> CreateAsync(Template template, CancellationToken ct = default)
    {
        Calls.Add(("create", template.Id));
        return Task.FromResult(template);
    }

    public Task<Template?> UpdateAsync(Template template, CancellationToken ct = default)
    {
        Calls.Add(("update", template.Id));
        return Task.FromResult<Template?>(template);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        Calls.Add(("delete", id));
        return Task.FromResult(true);
    }

    public Task<TemplateValidation?> ValidateVersionAsync(Guid versionId, CancellationToken ct = default)
    {
        Calls.Add(("validate", versionId));
        if (ValidationFailure is not null) throw ValidationFailure;
        return Task.FromResult(Validation);
    }
}

[Trait(Spec.SPEC, "3315")]
[Trait(Spec.SC, "03")]
[UserStory("03", "Template effects read the server and dispatch the reducer's messages")]
public class WhenHandlingTemplateEffects
{
    private static Template TemplateWithVersions(string name, params string[] versions)
    {
        var id = Guid.NewGuid();
        return new Template
        {
            Id = id,
            Name = name,
            Versions = versions
                .Select(version => new TemplateVersion { Id = Guid.NewGuid(), Name = version, TemplateId = id })
                .ToList(),
        };
    }

    [Trait(Spec.UAC, "03")]
    [Fact]
    public async Task ItShouldLoadTheLibraryAndCacheEveryVersion()
    {
        var template = TemplateWithVersions("EntityClass", "v1", "v2");
        var library = new StubLibrary { Templates = [template] };
        var effects = new TemplateEffects(library);
        var store = new RecordingStore();

        await effects.LoadAsync(store);

        library.Calls.Should().ContainSingle().Which.Call.Should().Be("list");
        var loaded = store.Dispatched.Should().ContainSingle().Which.Should().BeOfType<TemplatesLoaded>().Subject;
        loaded.AllItems.Should().ContainSingle().Which.Name.Should().Be("EntityClass");
        loaded.VersionCache.Should().HaveCount(2);
    }

    [Trait(Spec.UAC, "03")]
    [Fact]
    public async Task ItShouldSelectATemplateAndLoadItsVersionsNewestFirst()
    {
        var template = TemplateWithVersions("EntityClass", "v1", "v2");
        var library = new StubLibrary { Templates = [template] };
        var effects = new TemplateEffects(library);
        var store = new RecordingStore();

        await effects.SelectNodeAsync(store, new TemplateSelectNode("t", "EntityClass", "template", template.Id));

        store.Dispatched.Should().HaveCount(2);
        store.Dispatched[0].Should().BeOfType<TemplateSelectNode>();
        var loaded = store.Dispatched[1].Should().BeOfType<TemplateVersionsLoaded>().Subject;
        loaded.Versions.Select(version => version.Name).Should().Equal("v2", "v1");
    }

    [Trait(Spec.UAC, "03")]
    [Fact]
    public async Task ItShouldReportAValidationOutcome()
    {
        var library = new StubLibrary();
        var effects = new TemplateEffects(library);
        var store = new RecordingStore();

        await effects.ValidateVersionAsync(store, Guid.NewGuid());

        store.Dispatched.Should().HaveCount(2);
        store.Dispatched[0].Should().BeOfType<TemplateValidateStarted>();
        store.Dispatched[1].Should().BeOfType<TemplateValidateCompleted>()
            .Which.Result!.Valid.Should().BeTrue();
    }

    [Trait(Spec.UAC, "03")]
    [Fact]
    public async Task ItShouldCarryTheServersRefusalIntoTheValidationMessage()
    {
        var library = new StubLibrary { ValidationFailure = new InvalidOperationException("no test data") };
        var effects = new TemplateEffects(library);
        var store = new RecordingStore();

        await effects.ValidateVersionAsync(store, Guid.NewGuid());

        var completed = store.Dispatched.Should().ContainSingle(action => action is TemplateValidateCompleted)
            .Which.Should().BeOfType<TemplateValidateCompleted>().Subject;
        completed.Result.Should().BeNull();
        completed.Error.Should().Be("no test data");
    }

    [Trait(Spec.UAC, "03")]
    [Fact]
    public async Task ItShouldDeleteAndReloadSoTheListCannotGoStale()
    {
        var library = new StubLibrary();
        var effects = new TemplateEffects(library);
        var store = new RecordingStore();

        await effects.DeleteAsync(store, Guid.NewGuid());

        library.Calls.Select(call => call.Call).Should().Equal("delete", "list");
        store.Dispatched.Should().Contain(action => action is TemplateDeleteCompleted);
        store.Dispatched.Should().Contain(action => action is TemplatesLoaded);
    }
}
#endif
