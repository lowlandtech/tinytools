using System;

namespace LowlandTech.TinyTools
{
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
        protected abstract void Given();
        protected abstract T For();
        protected abstract void When();
        public virtual void Dispose() { }
    }
}
