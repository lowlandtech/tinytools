namespace LowlandTech.TinyTools.Tests.Infrastructure;

/// <summary>
/// Base class for pure unit tests that don't require external services.
/// Provides the Given/When/ArrangeAndAct BDD pattern.
/// </summary>
public abstract class UnitScenario
{
    protected virtual void Given() { }
    protected virtual void When() { }

    protected virtual Task GivenAsync() => Task.CompletedTask;
    protected virtual Task WhenAsync() => Task.CompletedTask;

    protected void ArrangeAndAct()
    {
        Given();
        When();
    }

    protected async Task ArrangeAndActAsync()
    {
        await GivenAsync();
        await WhenAsync();
    }
}
