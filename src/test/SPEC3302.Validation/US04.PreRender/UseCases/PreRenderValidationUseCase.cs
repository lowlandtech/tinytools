namespace LowlandTech.TinyTools.Tests.SPEC3302.Validation.US04.PreRender.UseCases;

/// <summary>Caller goal and captured result vocabulary only; executable API usage stays in each scenario.</summary>
public sealed class PreRenderValidationUseCase
{
    private bool executed;

    public Dictionary<string, object?> Cases { get; } = new(StringComparer.Ordinal);

    public void RunOnce(Action given, Action when)
    {
        lock (Cases)
        {
            if (executed) return;
            given();
            when();
            executed = true;
        }
    }
}
