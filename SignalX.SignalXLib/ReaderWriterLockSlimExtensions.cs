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

        sealed class ReadLockToken : IDisposable
        {
            ReaderWriterLockSlim locker;

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

        sealed class WriteLockToken : IDisposable
        {
            ReaderWriterLockSlim locker;

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