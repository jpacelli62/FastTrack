using System;
using System.Threading.Tasks;

namespace Faaast.Orm.Reader
{
    public class AsyncFaaastRowReader : BaseRowReader, IAsyncDisposable
    {

        public AsyncFaaastRowReader(AsyncFaaastCommand source) : base(source)
        {
        }

#if NET_5
        public ValueTask DisposeAsync() => new(this.Reader.DisposeAsync().ConfigureAwait(false));
#else
        public ValueTask DisposeAsync() => new(Task.CompletedTask);
#endif

        internal async Task PrepareAsync()
        {
            this.Reader = await this.Source.Command.ExecuteReaderAsync(this.Source.CommandBehavior, this.Source.CancellationToken);
            this.InitFields();
        }

        public async Task<bool> ReadAsync() => this.FillBuffer(await this.Reader.ReadAsync(this.Source.CancellationToken));

    }
}
