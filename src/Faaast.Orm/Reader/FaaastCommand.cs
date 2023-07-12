using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Faaast.Orm.Reader
{
    public sealed class FaaastCommand : IAsyncDisposable, IDisposable
    {
        public FaaastDb Db { get; set; }

        public DbCommand Command { get; set; }

        public int? CommandTimeout { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public CommandBehavior CommandBehavior { get; set; }

        public bool AutoClose { get; set; }

        public FaaastCommand(FaaastDb db, DbCommand command)
        {
            this.Db = db ?? throw new ArgumentNullException(nameof(db));
            this.Command = command ?? throw new ArgumentNullException(nameof(command));
            this.CancellationToken = CancellationToken.None;
            this.CommandBehavior = CommandBehavior.SequentialAccess;
            this.AutoClose = false;
        }

        public int ExecuteNonQuery()
        {
            using var command = this.Command;
            command.CommandTimeout = this.CommandTimeout ?? command.CommandTimeout;

            var result = 0;
            Exception ex = null;
            try
            {
                result = command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                ex = e;
            }
            finally
            {
                if (this.AutoClose)
                {
                    command.Connection.Close();
                }
            }

            return ex != null ? throw new FaaastOrmException(ex.Message, ex) : result;
        }

        public void ExecuteReader(Action<FaaastRowReader> stuff)
        {
            using var reader = new FaaastRowReader(this);
            reader.Prepare();
            stuff(reader);
        }

        public async Task<int> ExecuteNonQueryAsync()
        {

            using var command = this.Command;
            command.CommandTimeout = this.CommandTimeout ?? command.CommandTimeout;
            
            var result = 0;
            Exception ex = null;
            try
            {
                result = await this.Command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ex = e;
            }
            finally
            {
                if (this.AutoClose)
                {

#if NET_5
                    await command.Connection.CloseAsync().ConfigureAwait(false);
#else
                    command.Connection.Close();
#endif
                }
            }

            return ex != null ? throw new FaaastOrmException(ex.Message, ex) : result;
        }

        public async Task ExecuteReaderAsync(Func<AsyncFaaastRowReader, Task> stuff)
        {
            await using var reader = new AsyncFaaastRowReader(this);
            await reader.PrepareAsync();
            await stuff(reader);
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposing)
        {
            if (disposing && this.Command != null)
            {
                if (this.AutoClose)
                {
                    this.Command.Connection.Close();
                }

                this.Command.Dispose();
                this.Command = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsyncCore().ConfigureAwait(false);
            this.Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        internal async ValueTask DisposeAsyncCore()
        {
            if (this.Command != null)
            {
#if NET_5
                if (this.AutoClose)
                {
                    await this.Command.Connection.CloseAsync().ConfigureAwait(false);
                }
                await this.Command.DisposeAsync().ConfigureAwait(false);
                this.Command = null;
#else
                if (this.AutoClose)
                {
                    this.Command.Connection.Close();
                }

                this.Dispose();
#endif
            }

            await Task.CompletedTask;
        }

        public async Task<T> FirstOrDefaultAsync<T>()
        {
            T result = default;
            await this.ExecuteReaderAsync(async reader =>
            {
                var tReader = reader.AddReader<T>();
                if (await reader.ReadAsync())
                {
                    result = tReader.Value;
                }
            });

            return result;
        }

        public T FirstOrDefault<T>()
        {
            T result = default;
            this.ExecuteReader(reader =>
            {
                var tReader = reader.AddReader<T>();
                if (reader.Read())
                {
                    result = tReader.Value;
                }
            });

            return result;
        }

        public async Task<ICollection<T>> ToListAsync<T>()
        {
            var result = new List<T>();
            await this.ExecuteReaderAsync(async reader =>
            {
                var tReader = reader.AddReader<T>();
                while (await reader.ReadAsync())
                {
                    result.Add(tReader.Value);
                }
            });

            return result;
        }

        public ICollection<T> ToList<T>()
        {
            var result = new List<T>();
            this.ExecuteReader(reader =>
            {
                var tReader = reader.AddReader<T>();
                while (reader.Read())
                {
                    result.Add(tReader.Value);
                }
            });

            return result;
        }
    }
}
