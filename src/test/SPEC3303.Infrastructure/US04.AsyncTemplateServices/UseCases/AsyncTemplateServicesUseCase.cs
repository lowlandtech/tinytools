namespace LowlandTech.TinyTools.Tests.SPEC3303.Infrastructure.US04.AsyncTemplateServices.UseCases;

/// <summary>Caller goal and named results only; executable setup and calls stay in SC01.</summary>
public sealed class AsyncTemplateServicesUseCase
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
