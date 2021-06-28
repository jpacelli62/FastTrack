using System;
using System.Threading;

namespace FastTrack.Metadata
{
    public class ReadWriteSync
    {
        public sealed class SyncToken : IDisposable
        {
            private Action OnRelease { get; set; }

            public SyncToken(Action onRelease)
            {
                OnRelease = onRelease;
            }

            public void Dispose()
            {
                OnRelease();
            }
        }

        private ReaderWriterLock SyncObject { get; set; }

        public ReadWriteSync()
        {
            SyncObject = new ReaderWriterLock();
        }

        public SyncToken ReadAccess(int timeout)
        {
            this.SyncObject.AcquireReaderLock(timeout);
            return new SyncToken(() => this.SyncObject.ReleaseReaderLock());
        }

        public SyncToken WriteAccess(int timeout)
        {
            this.SyncObject.AcquireWriterLock(timeout);
            return new SyncToken(() => this.SyncObject.ReleaseWriterLock());
        }

        public SyncToken UpgradeToWriteAccess(int timeout)
        {
            var token = this.SyncObject.UpgradeToWriterLock(timeout);
            return new SyncToken(() => this.SyncObject.DowngradeFromWriterLock(ref token));
        }
    }
}
