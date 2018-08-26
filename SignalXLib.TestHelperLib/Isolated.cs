namespace SignalXLib.TestHelperLib
{
    using System;

    public sealed class Isolated<T> : IDisposable
        where T : MarshalByRefObject
    {
        private AppDomain _domain;

        public Isolated()
        {
            this._domain = AppDomain.CreateDomain(
                "Isolated:" + Guid.NewGuid(),
                null,
                AppDomain.CurrentDomain.SetupInformation);

            Type type = typeof(T);

            this.Value = (T)this._domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }

        public T Value { get; }

        public void Dispose()
        {
            if (this._domain != null)
            {
                try
                {
                    try
                    {
                        AppDomain.Unload(this._domain);
                    }
                    catch (CannotUnloadAppDomainException)
                    {
                        GC.Collect();
                        AppDomain.Unload(this._domain);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                this._domain = null;
            }
        }
    }
}