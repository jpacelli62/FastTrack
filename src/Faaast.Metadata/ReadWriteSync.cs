using System;
using System.Threading;

namespace Faaast.Metadata
{
    public class ReadWriteSync
    {
        public int DefaultTimeout { get; set; } = 10000;

        public sealed class SyncToken : IDisposable
        {
            private Action OnRelease { get; set; }

            public SyncToken(Action onRelease) => this.OnRelease = onRelease;

            public void Dispose() => this.OnRelease();
        }

        private ReaderWriterLock SyncObject { get; set; }

        public ReadWriteSync() => this.SyncObject = new ReaderWriterLock();

        public ReadWriteSync(int timeout) : this() => this.DefaultTimeout = timeout;

        public SyncToken ReadAccess(int? timeout = null)
        {
            timeout ??= this.DefaultTimeout;
            this.SyncObject.AcquireReaderLock(timeout.Value);
            return new SyncToken(() => this.SyncObject.ReleaseReaderLock());
        }

        public SyncToken WriteAccess(int? timeout = null)
        {
            timeout ??= this.DefaultTimeout;
            this.SyncObject.AcquireWriterLock(timeout.Value);
            return new SyncToken(() => this.SyncObject.ReleaseWriterLock());
        }

        public SyncToken UpgradeToWriteAccess(int? timeout = null)
        {
            timeout ??= this.DefaultTimeout;
            var token = this.SyncObject.UpgradeToWriterLock(timeout.Value);
            return new SyncToken(() => this.SyncObject.DowngradeFromWriterLock(ref token));
        }
    }
}
