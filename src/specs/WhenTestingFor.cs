namespace LowlandTech.Specs;

public abstract class WhenTestingFor<T> : IDisposable
{
    protected T Sut;

    /// <summary>
    /// Gets or sets whether to automatically apply [Given] preconditions.
    /// Default is false for backward compatibility.
    /// </summary>
    protected virtual bool AutoExecuteGivens => false;

    public WhenTestingFor()
    {
        SetupData();
        Given();
        
        // Auto-apply AFTER Given() has created the state object
        if (AutoExecuteGivens)
        {
            this.AutoApplyGivens();
        }
        
        Sut = For();
        When();
    }

    protected virtual void SetupData() { }
    protected virtual void Given() { }
    protected abstract T For();
    protected virtual void When() { }
    public virtual void Dispose() { }
}
