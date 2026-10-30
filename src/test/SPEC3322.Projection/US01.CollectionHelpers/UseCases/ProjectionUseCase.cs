namespace LowlandTech.TinyTools.Tests.SPEC3322.Projection.US01.CollectionHelpers.UseCases;

/// <summary>Caller goal and captured result vocabulary only; executable API usage stays in each scenario.</summary>
public sealed class ProjectionUseCase
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

    /// <summary>The item shape the collection helpers read members from.</summary>
    public sealed class Item
    {
        public string Name { get; set; } = string.Empty;

        public string Team { get; set; } = string.Empty;

        public bool Active { get; set; }

        public decimal Score { get; set; }
    }
}
