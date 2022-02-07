using System;

namespace Faaast.Orm.Reader
{
    public sealed class FaaastRowReader : BaseRowReader, IDisposable
    {
        public FaaastRowReader(FaaastCommand source) : base(source)
        {
        }

        public void Dispose()
        {
#if NET_5
            this.Reader.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
#endif
            // Do nothing
        }

        internal void Prepare()
        {
            this.Reader = this.Source.Command.ExecuteReader(this.Source.CommandBehavior);
            this.InitFields();
        }

        public bool Read() => this.FillBuffer(this.Reader.Read());
    }
}
