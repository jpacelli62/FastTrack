using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Faaast.Orm.Reader
{
    public class AsyncFaaastCommand : BaseCommand, IAsyncDisposable, IDisposable
    {
        public AsyncFaaastCommand( FaaastDb db, DbConnection dbConnection, string commandText, object parameters = null) : 
            base(db, dbConnection, commandText, parameters)
        {

        }

        public async Task<int> ExecuteNonQueryAsync()
        {
            using var dbCommand = this.CreateInternalCommand();
            var result = 0;
            Exception ex = null;
            try
            {
                result = await dbCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch(Exception e)
            {
                ex = e;
            }
            finally
            {
                if (this.AutoClose)
                {
#if NET_5
                    await this.Connection.CloseAsync().ConfigureAwait(false);
#else
                    this.Connection.Close();
#endif
                }
            }

            return ex != null ? throw ex : result;
        }

        public async Task<AsyncFaaastRowReader> ExecuteReaderAsync()
        {
            this.CreateInternalCommand();
            var reader = new AsyncFaaastRowReader(this);
            await reader.PrepareAsync();
            return reader;
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.Command != null)
            {
                this.Command.Dispose();
                this.Command = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsyncCore().ConfigureAwait(false);
            this.Dispose(disposing: false);
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (this.Command != null)
            {
#if NET_5
                await this.Command.DisposeAsync().ConfigureAwait(false);
                this.Command = null;

#else
                this.Dispose();
#endif
            }

            await Task.CompletedTask;
        }
    }
}
