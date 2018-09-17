namespace SignalXLib.Lib
{
    using System;
    using System.Threading;

    public static class SingleWriterExtensions
    {
        public static IDisposable Read(this ReaderWriterLockSlim locker)
        {
            return new ReadLockToken(locker);
        }

        public static IDisposable Write(this ReaderWriterLockSlim locker)
        {
            return new WriteLockToken(locker);
        }

        private sealed class ReadLockToken : IDisposable
        {
            private ReaderWriterLockSlim locker;

            public ReadLockToken(ReaderWriterLockSlim sync)
            {
                this.locker = sync;
                sync.EnterReadLock();
            }

            public void Dispose()
            {
                if (this.locker == null)
                    return;
                this.locker.ExitReadLock();
                this.locker = null;
            }
        }

        private sealed class WriteLockToken : IDisposable
        {
            private ReaderWriterLockSlim locker;

            public WriteLockToken(ReaderWriterLockSlim sync)
            {
                this.locker = sync;
                sync.EnterWriteLock();
            }

            public void Dispose()
            {
                if (this.locker == null)
                    return;
                this.locker.ExitWriteLock();
                this.locker = null;
            }
        }
    }
}