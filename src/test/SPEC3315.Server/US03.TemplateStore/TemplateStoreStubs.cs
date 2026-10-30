#if NET10_0_OR_GREATER
using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Core.Entities;
using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US03.TemplateStore;

/// <summary>A store that records what the effects dispatch.</summary>
public sealed class RecordingStore : ITemplateStore
{
    public TemplateState State { get; } = new();
    public List<object> Dispatched { get; } = [];
    public void Dispatch(object action) => Dispatched.Add(action);
}

/// <summary>A library that answers with fixed data and records the calls.</summary>
public sealed class StubLibrary : ITemplateLibrary
{
    public List<Template> Templates { get; init; } = [];
    public TemplateValidation? Validation { get; init; } = new(true, "out", "out");
    public Exception? ValidationFailure { get; init; }
    public List<string> Calls { get; } = [];

    public Task<List<Template>> ListAsync(CancellationToken ct = default)
    {
        Calls.Add("list");
        return Task.FromResult(Templates);
    }

    public Task<Template?> GetAsync(Guid id, CancellationToken ct = default)
    {
        Calls.Add("get");
        return Task.FromResult(Templates.FirstOrDefault(template => template.Id == id));
    }

    public Task<Template> CreateAsync(Template template, CancellationToken ct = default)
    {
        Calls.Add("create");
        return Task.FromResult(template);
    }

    public Task<Template?> UpdateAsync(Template template, CancellationToken ct = default)
    {
        Calls.Add("update");
        return Task.FromResult<Template?>(template);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        Calls.Add("delete");
        return Task.FromResult(true);
    }

    public Task<TemplateValidation?> ValidateVersionAsync(Guid versionId, CancellationToken ct = default)
    {
        Calls.Add("validate");
        if (ValidationFailure is not null) throw ValidationFailure;
        return Task.FromResult(Validation);
    }
}
#endif