namespace LowlandTech.TinyTools;

public abstract class WhenTestingFor<T> : IDisposable
{
    protected T Sut;

    public WhenTestingFor()
    {
        SetupData();
        Given();
        Sut = For();
        When();
    }

    protected virtual void SetupData() { }
    protected virtual void Given() { }
    protected abstract T For();
    protected virtual void When() { }
    public virtual void Dispose() { }
}
