namespace LowlandTech.TinyTools.Tests.SPEC3300.TemplateEngine.US03.Operators.UseCases;

/// <summary>Caller goal: render comparison boundaries through the public engine without changing it.</summary>
public sealed class ComparisonBoundaryUseCase
{
    private bool _executed;
    public Dictionary<string, string[]> Cases { get; } = new(StringComparer.Ordinal);

    // Only orchestration lives here. Both executable steps remain in each scenario source.
    public void RunOnce(Action given, Action when)
    {
        lock (Cases)
        {
            if (_executed) return;
            given();
            when();
            _executed = true;
        }
    }
}
