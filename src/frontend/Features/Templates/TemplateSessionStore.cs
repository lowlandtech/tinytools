using LowlandTech.TinyTools.Frontend.Features.Templates;
using LowlandTech.TinyTools.Frontend.Services.Http;

namespace LowlandTech.TinyTools.Frontend.Features.Templates;

/// <summary>
/// The store of the templates feature: it holds the feature state and tells the
/// page when an effect changed it.
/// <para>
/// The store travels with the package rather than with a host. A published
/// client composes this page from the package, so the page must resolve its
/// services from the package too; a store that stayed behind in a development
/// host would leave the page unconstructible wherever it is deployed.
/// </para>
/// </summary>
public sealed class TemplateSessionStore : ITemplateStore
{
    public TemplateState State { get; private set; } = new();

    /// <summary>Raised after an action changed the state.</summary>
    public event Action? Changed;

    public void Dispatch(object action)
    {
        State = TemplateReducer.Reduce(State, action);
        Changed?.Invoke();
    }
}
