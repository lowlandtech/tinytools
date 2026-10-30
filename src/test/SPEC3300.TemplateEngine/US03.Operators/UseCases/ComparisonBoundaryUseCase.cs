namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US03.Operators.UseCases;

/// <summary>Caller goal: render comparison boundaries through the public engine without changing it.</summary>
public sealed class ComparisonBoundaryUseCase
{
    private bool executed;
    public Dictionary<string, object?> Cases { get; } = new(StringComparer.Ordinal);

    // Only orchestration lives here. Both executable steps remain in each scenario source.
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
