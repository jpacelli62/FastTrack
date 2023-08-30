using System;
using System.Threading.Tasks;

namespace Faaast.Orm.Reader
{
    public class AsyncFaaastRowReader : BaseRowReader
    {

        public AsyncFaaastRowReader(FaaastCommand source) : base(source)
        {
        }

        internal async Task PrepareAsync()
        {
            this.Source.Command.CommandTimeout = this.Source.CommandTimeout ?? this.Source.Command.CommandTimeout;
            this.Reader = await this.Source.Command.ExecuteReaderAsync(this.Source.CommandBehavior, this.Source.CancellationToken);
            this.InitFields();
        }

        public async Task<bool> ReadAsync() => this.FillBuffer(await this.Reader.ReadAsync(this.Source.CancellationToken));

    }
}
