using LowlandTech.TinyTools.Frontend.Features.Templates;

namespace LowlandTech.TinyTools.App.Components;

/// <summary>
/// The app's store: it holds the feature state and tells the page when an effect
/// changed it. The frontend package defines the seam, so this stays a thin host.
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
