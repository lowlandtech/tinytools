namespace LowlandTech.TinyTools.Tests.Infrastructure;

/// <summary>
/// Base class for TinyTools unit tests that construct a System Under Test (SUT)
/// via the For() factory method. Bridges the legacy WhenTestingFor&lt;T&gt; pattern
/// with the modern ArrangeAndAct scenario pattern.
/// </summary>
public abstract class TinyToolsScenario<T> : UnitScenario, IDisposable
{
    protected T Sut = default!;

    protected virtual void SetupData() { }
    protected abstract T For();
    protected virtual void Act() { }

    protected override void Given()
    {
        SetupData();
        base.Given();
    }

    protected override void When()
    {
        Sut = For();
        Act();
    }

    public virtual void Dispose() { }
}
