using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Faaast.Orm.Reader;

namespace Faaast.Orm
{
    public static partial class DbExtensions
    {
        public static FaaastCommand Query(this FaaastDb db, string sql, object parameters = null,
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null,
            CancellationToken cancellationToken = default)
        {
            bool handleConnection = connection == null;

            return new(db.DbStore[db.Connection.Name],
            db.Mapper,
            connection ?? db.CreateConnection(),
            sql,
            parameters,
            transaction,
            commandTimeout,
            commandType,
            handleConnection,
            cancellationToken);
        }

        internal static Task TryPrepareAsync(this DbCommand dbCommand, CancellationToken cancellationToken)
        {
#if NET_5
            return dbCommand.PrepareAsync(cancellationToken);
#endif
            if (!cancellationToken.IsCancellationRequested)
            {
                dbCommand.Prepare();
            }

            return Task.CompletedTask;
        }

        internal static Task TryCloseAsync(this DbConnection connection, CancellationToken cancellationToken)
        {
#if NET_5
            return connection.CloseAsync(cancellationToken);
#else
            if (!cancellationToken.IsCancellationRequested)
            {
                connection.Close();
            }

            return Task.CompletedTask;
#endif
        }

        internal static Task TryDisposeAsync(this DbConnection connection, CancellationToken cancellationToken)
        {
#if NET_5
            return connection.DisposeAsync(cancellationToken);
#else
            if (!cancellationToken.IsCancellationRequested)
            {
                connection.Dispose();
            }

            return Task.CompletedTask;
#endif
        }

        public static async Task<int> ExecuteAsync(this FaaastCommand command)
        {
            var result = 0;
            if (command.HandleConnection)
            {
                await command.Connection.OpenAsync(command.CancellationToken).ConfigureAwait(false);
            }

            using (var dbCommand = await command.PreprareAsync())
            {
                result = await dbCommand.ExecuteNonQueryAsync(command.CancellationToken).ConfigureAwait(false);
            }

            if (command.HandleConnection)
            {
                await TryCloseAsync(command.Connection, command.CancellationToken).ConfigureAwait(false);
                await command.Connection.TryDisposeAsync(command.CancellationToken);
            }

            return result;
        }

        internal static IEnumerable<T> Read<T>(this FaaastCommand command)
        {
            if (command.HandleConnection)
            {
                command.Connection.Open();
            }

            using (var dbCommand = command.Preprare())
            {
                var dbReader = dbCommand.ExecuteReader(command.CommandBehavior);
                var composite = new CompositeReader(command, dbReader, typeof(T));

                while (dbReader.Read())
                {
                    yield return (T)composite.Read(dbReader)[0];
                }
            }

            if (command.HandleConnection)
            {
                command.Connection.Close();
                command.Connection.Dispose();
            }
        }

        //internal static IEnumerable<dynamic> ReadDynamic(this FaaastCommand command)
        //{
        //    if (command.HandleConnection)
        //    {
        //        command.Connection.Open();
        //    }

        //    using (var dbCommand = command.SetupCommand())
        //    {
        //        dbCommand.Prepare();
        //        var dbReader = dbCommand.ExecuteReader(command.CommandBehavior);
        //        var composite = CompositeReader.DynamicReader(command, dbReader);

        //        while (dbReader.Read())
        //        {
        //            yield return composite.Read(dbReader)[0];
        //        }
        //    }

        //    if (command.HandleConnection)
        //    {
        //        command.Connection.Close();
        //        command.Connection.Dispose();
        //    }
        //}

        internal static IEnumerable<object[]> Read(this FaaastCommand command, params Type[] types)
        {
            if (command.HandleConnection)
            {
                command.Connection.Open();
            }

            using (IDbCommand dbCommand = command.Preprare())
            {
                var dbReader = dbCommand.ExecuteReader(command.CommandBehavior);
                var composite = new CompositeReader(command, dbReader, types);

                while (dbReader.Read())
                {
                    yield return composite.Read(dbReader);
                }
            }

            if (command.HandleConnection)
            {
                command.Connection.Close();
                command.Connection.Dispose();
            }
        }

        public static async IAsyncEnumerable<T> ReadAsync<T>(this FaaastCommand command)
        {
            if (command.HandleConnection)
            {
                await command.Connection.OpenAsync(command.CancellationToken).ConfigureAwait(false);
            }

            using (var dbCommand = await command.PreprareAsync())
            {
                var dbReader = await dbCommand.ExecuteReaderAsync(command.CommandBehavior, command.CancellationToken).ConfigureAwait(false);
                var composite = new CompositeReader(command, dbReader, typeof(T));

                while (await dbReader.ReadAsync(command.CancellationToken).ConfigureAwait(false))
                {
                    yield return (T)composite.Read(dbReader)[0];
                }
            }

            if (command.HandleConnection)
            {
                await TryCloseAsync(command.Connection, command.CancellationToken).ConfigureAwait(false);
                await command.Connection.TryDisposeAsync(command.CancellationToken);
            }
        }

        public static async IAsyncEnumerable<object[]> ReadAsync(this FaaastCommand command, params Type[] types)
        {
            if (command.HandleConnection)
            {
                await command.Connection.OpenAsync(command.CancellationToken).ConfigureAwait(false);
            }

            using (var dbCommand = await command.PreprareAsync())
            {
                var dbReader = await dbCommand.ExecuteReaderAsync(command.CommandBehavior, command.CancellationToken).ConfigureAwait(false);
                var composite = new CompositeReader(command, dbReader, types);

                while (await dbReader.ReadAsync(command.CancellationToken).ConfigureAwait(false))
                {
                    yield return composite.Read(dbReader);
                }
            }

            if (command.HandleConnection)
            {
                await TryCloseAsync(command.Connection, command.CancellationToken).ConfigureAwait(false);
                await command.Connection.TryDisposeAsync(command.CancellationToken);
            }
        }

        public static T FirstOrDefault<T>(this FaaastCommand command)
        {
            var enumerator = Read<T>(command).GetEnumerator();
            return enumerator.MoveNext() ? enumerator.Current : default;
        }

        public static object[] FirstOrDefault(this FaaastCommand command, params Type[] types)
        {
            var enumerator = Read(command, types).GetEnumerator();
            return enumerator.MoveNext() ? enumerator.Current : default;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this FaaastCommand command)
        {
            var enumerator = ReadAsync<T>(command).ConfigureAwait(false).GetAsyncEnumerator();
            return await enumerator.MoveNextAsync() ? enumerator.Current : default;
        }

        public static async Task<object[]> FirstOrDefaultAsync(this FaaastCommand command, params Type[] types)
        {
            var enumerator = ReadAsync(command, types).ConfigureAwait(false).GetAsyncEnumerator();
            return await enumerator.MoveNextAsync() ? enumerator.Current : default;
        }

        public static T Single<T>(this FaaastCommand command)
        {
            var row = 0;
            T result = default;
            foreach (var item in Read<T>(command))
            {
                result = item;
                row++;
                if (row > 1)
                {
                    throw new InvalidOperationException("Seqence contains more than one element");
                }
            }

            return result;
        }

        public static object[] Single(this FaaastCommand command, params Type[] types)
        {
            var row = 0;
            object[] result = default;
            foreach (var item in Read(command, types))
            {
                result = item;
                row++;
                if (row > 1)
                {
                    throw new InvalidOperationException("Seqence contains more than one element");
                }
            }

            return result;
        }

        public static async Task<T> SingleAsync<T>(this FaaastCommand command)
        {
            var row = 0;
            T result = default;
            await foreach (var item in ReadAsync<T>(command).ConfigureAwait(false))
            {
                result = item;
                row++;
                if (row > 1)
                {
                    throw new InvalidOperationException("Seqence contains more than one element");
                }
            }

            return result;
        }

        public static async Task<object[]> SingleAsync(this FaaastCommand command, params Type[] types)
        {
            var row = 0;
            object[] result = default;
            await foreach (var item in ReadAsync(command, types).ConfigureAwait(false))
            {
                result = item;
                row++;
                if (row > 1)
                {
                    throw new InvalidOperationException("Seqence contains more than one element");
                }
            }

            return result;
        }

        public static IEnumerable<object[]> Fetch(this FaaastCommand command, params Type[] types) => Read(command, types);

        public static IEnumerable<T> Fetch<T>(this FaaastCommand command) => Read<T>(command);

        //public static IEnumerable<dynamic> Fetch(this FaaastCommand command) => ReadDynamic(command);

        public static IAsyncEnumerable<T> FetchAsync<T>(this FaaastCommand command) => ReadAsync<T>(command);

        public static IAsyncEnumerable<object[]> FetchAsync(this FaaastCommand command, params Type[] types) => ReadAsync(command, types);
    }
}
