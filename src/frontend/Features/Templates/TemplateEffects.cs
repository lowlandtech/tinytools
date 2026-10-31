using LowlandTech.TinyTools.Core.Abstractions;
using LowlandTech.TinyTools.Core.Entities;

namespace LowlandTech.TinyTools.Frontend.Features.Templates;

/// <summary>
/// The store the effects write to. The page supplies it, so the effects hold no
/// dependency on a particular store implementation.
/// </summary>
public interface ITemplateStore
{
    TemplateState State { get; }

    void Dispatch(object action);

    /// <summary>Raised after a dispatch changed the state, so a view can re-render.</summary>
    event Action? Changed;
}

/// <summary>
/// The side of the templates feature that talks to the server: it reads through
/// <see cref="ITemplateLibrary"/> and dispatches the messages the reducer already
/// handles.
///
/// Foundry's effects additionally synchronised a graph store and persisted open
/// tabs. Neither surface exists in this repository yet, so they are not ported
/// here; a thin adapter can add them without changing these handlers.
/// </summary>
public sealed class TemplateEffects(ITemplateLibrary library)
{
    /// <summary>Loads the library and caches every version with its template.</summary>
    public async Task LoadAsync(ITemplateStore store, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        var templates = await library.ListAsync(ct);
        var cache = new Dictionary<Guid, TemplateVersion?>();
        foreach (var template in templates)
        {
            foreach (var version in template.Versions)
                cache[version.Id] = version;
        }

        store.Dispatch(new TemplatesLoaded(templates, cache));
    }

    /// <summary>
    /// Selects a template node: loads its versions in the reducer's order and
    /// dispatches both the selection and the loaded versions.
    /// </summary>
    public async Task SelectNodeAsync(ITemplateStore store, TemplateSelectNode action, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(action);

        var template = await library.GetAsync(action.EntityId, ct);
        var versions = template?.Versions
            .OrderByDescending(version => version.Name, StringComparer.Ordinal)
            .ToList() ?? [];

        store.Dispatch(action);
        store.Dispatch(new TemplateVersionsLoaded(action.EntityId, versions));
    }

    /// <summary>
    /// Validates one version and reports the outcome, including the server's own
    /// refusal text, so the page can show why a version did not validate.
    /// </summary>
    public async Task ValidateVersionAsync(ITemplateStore store, Guid versionId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        store.Dispatch(new TemplateValidateStarted());

        try
        {
            var validation = await library.ValidateVersionAsync(versionId, ct);
            if (validation is null)
                store.Dispatch(new TemplateValidateCompleted(null, "The version no longer exists."));
            else
                store.Dispatch(new TemplateValidateCompleted(validation, null));
        }
        catch (InvalidOperationException exception)
        {
            store.Dispatch(new TemplateValidateCompleted(null, exception.Message));
        }
    }

    /// <summary>Deletes a template and reloads the library so the list cannot go stale.</summary>
    public async Task DeleteAsync(ITemplateStore store, Guid templateId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        if (!await library.DeleteAsync(templateId, ct))
        {
            // A template that is already gone is not an error; the list is still reloaded.
        }

        store.Dispatch(new TemplateDeleteCompleted(templateId.ToString()));
        await LoadAsync(store, ct);
    }
}
