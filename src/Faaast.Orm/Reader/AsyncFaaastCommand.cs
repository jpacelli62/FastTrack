using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Faaast.Orm.Reader
{
    public class AsyncFaaastCommand : BaseCommand, IAsyncDisposable
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

        public ValueTask DisposeAsync()
        {
            if (this.Command != null)
            {
#if NET_5
                return new (this.Command.DisposeAsync().ConfigureAwait(false));
#endif
            }

            return new(Task.CompletedTask);
        }
    }
}
