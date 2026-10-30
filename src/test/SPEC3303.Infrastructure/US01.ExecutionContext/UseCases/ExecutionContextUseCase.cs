namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US01.ExecutionContext.UseCases;

/// <summary>Caller goal and captured result vocabulary only; executable API usage stays in the scenarios.</summary>
public sealed class ExecutionContextUseCase
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
