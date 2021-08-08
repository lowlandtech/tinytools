using System;

namespace LowlandTech.TinyTools
{
    public abstract class WhenTestingFor<T> : IDisposable
    {
        protected T Sut;
        private readonly object _sut = null;

        public WhenTestingFor()
        {
            SetupData();
            Given();
            Sut = For();
            When();
        }

        protected virtual void SetupData() { }
        protected virtual void Given() { }
        protected virtual T For() { return (T)_sut; }
        protected virtual void When() { }
        public virtual void Dispose() { }
    }
}
