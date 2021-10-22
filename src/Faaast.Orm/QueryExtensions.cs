using Faaast.DatabaseModel;
using Faaast.Metadata;
using Faaast.Orm.Reader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Faaast.Orm
{
    public static partial class QueryExtensions
    {
        private static readonly ConcurrentDictionary<Type, ObjectReader> Parsers = new ConcurrentDictionary<Type, ObjectReader>();

        public static ObjectReader GetRowParser(this FaaastCommand command, Type type)
        {
            return Parsers.GetOrAdd(type, x =>
            {
                var db = ((MetaModel<IDatabase>)command.Database).Get(Meta.Mapping);
                if (!db.TypeToMapping.ContainsKey(type))
                    throw new ArgumentException($"No mapping found for type \"{type.FullName}\"");

                var mapping = db.TypeToMapping[type];
                return new ObjectReader(mapping.Table.Columns, mapping);
            });
        }

        public static FaaastCommand Query(this FaaastDb db, string sql, object parameters = null,
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null,
            CancellationToken cancellationToken = default)
        {
            return new FaaastCommand(db.DbStore[db.Connection.Name],
            db.Mapper,
            connection ?? db.CreateConnection(),
            sql,
            parameters,
            transaction,
            commandTimeout,
            commandType,
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

        public static async Task<int> ExecuteAsync(this FaaastCommand command)
        {
            int result = 0;
            using (command.Connection)
            {
                await command.Connection.OpenAsync(command.CancellationToken).ConfigureAwait(false);
                using (DbCommand dbCommand = command.SetupCommand())
                {
                    await dbCommand.TryPrepareAsync(command.CancellationToken).ConfigureAwait(false);
                    result = await dbCommand.ExecuteNonQueryAsync(command.CancellationToken).ConfigureAwait(false);
                }
                await TryCloseAsync(command.Connection, command.CancellationToken).ConfigureAwait(false);
            }
            return result;
        }

        internal static IEnumerable<T> Read<T>(this FaaastCommand command)
        {
            using (command.Connection)
            {
                command.Connection.Open();
                using (DbCommand dbCommand = command.SetupCommand())
                {
                    var type = typeof(T);
                    dbCommand.Prepare();
                    ObjectReader rowParsers = command.GetRowParser(type);
                    var dbReader = dbCommand.ExecuteReader(command.CommandBehavior);
                    int[] indices = ResolveColumnOrder(dbReader, rowParsers);
                    while (dbReader.Read())
                    {
                        yield return (T)rowParsers.Read(dbReader, indices, 0);
                    }
                }
                command.Connection.Close();
            }

        }

        internal static IEnumerable<object[]> Read(this FaaastCommand command, params Type[] types)
        {
            using (command.Connection)
            {
                command.Connection.Open();
                using (IDbCommand dbCommand = command.SetupCommand())
                {
                    dbCommand.Prepare();
                    ObjectReader[] rowParsers = new ObjectReader[types.Length];
                    for (int i = 0; i < types.Length; i++)
                    {
                        rowParsers[i] = command.GetRowParser(types[i]);
                    }
                    var dbReader = dbCommand.ExecuteReader(command.CommandBehavior);
                    int[] indexes = ResolveColumnOrder(dbReader, rowParsers);

                    while (dbReader.Read())
                    {
                        object[] results = new object[types.Length];
                        int start = 0;
                        for (int i = 0; i < types.Length; i++)
                        {
                            results[i] = rowParsers[i].Read(dbReader, indexes, start);
                            start += rowParsers[i].ColumnsReaders.Length;
                        }

                        yield return results;
                    }
                }
                command.Connection.Close();
            }
        }

        internal static async IAsyncEnumerable<T> ReadAsync<T>(this FaaastCommand command)
        {
            using (command.Connection)
            {
                await command.Connection.OpenAsync(command.CancellationToken).ConfigureAwait(false);
                using (DbCommand dbCommand = command.SetupCommand())
                {
                    var type = typeof(T);
                    await dbCommand.TryPrepareAsync(command.CancellationToken).ConfigureAwait(false);
                    ObjectReader rowParsers = command.GetRowParser(type);
                    var dbReader = await dbCommand.ExecuteReaderAsync(command.CommandBehavior, command.CancellationToken).ConfigureAwait(false);
                    int[] indices = ResolveColumnOrder(dbReader, rowParsers);
                    while (await dbReader.ReadAsync(command.CancellationToken).ConfigureAwait(false))
                    {
                        yield return (T)rowParsers.Read(dbReader, indices, 0);
                    }
                }
                await TryCloseAsync(command.Connection, command.CancellationToken).ConfigureAwait(false);
            }
        }

        internal static async IAsyncEnumerable<object[]> ReadAsync(this FaaastCommand command, params Type[] types)
        {
            using (command.Connection)
            {
                await command.Connection.OpenAsync(command.CancellationToken).ConfigureAwait(false);
                using (DbCommand dbCommand = command.SetupCommand())
                {
                    await dbCommand.TryPrepareAsync(command.CancellationToken).ConfigureAwait(false);
                    ObjectReader[] rowParsers = new ObjectReader[types.Length];
                    for (int i = 0; i < types.Length; i++)
                    {
                        rowParsers[i] = command.GetRowParser(types[i]);
                    }
                    var dbReader = await dbCommand.ExecuteReaderAsync(command.CommandBehavior, command.CancellationToken).ConfigureAwait(false);
                    int[] indexes = ResolveColumnOrder(dbReader, rowParsers);

                    while (await dbReader.ReadAsync(command.CancellationToken).ConfigureAwait(false))
                    {
                        object[] results = new object[types.Length];
                        int start = 0;
                        for (int i = 0; i < types.Length; i++)
                        {
                            results[i] = rowParsers[i].Read(dbReader, indexes, start);
                            start += rowParsers[i].ColumnsReaders.Length;
                        }

                        yield return results;
                    }
                }

                await TryCloseAsync(command.Connection, command.CancellationToken).ConfigureAwait(false);
            }
        }

        internal static int[] ResolveColumnOrder(IDataReader reader, params ObjectReader[] objects)
        {
            int startingindex = 0;
            int fieldCount = reader.FieldCount;
            int[] order = new int[fieldCount];
            for (int i = 0; i < fieldCount; i++)//3
            {
                order[i] = -1;
            }

            foreach (var columns in objects)
            {
                int columnsCount = columns.ColumnsNames.Length;
                int nextIndex = startingindex + columnsCount;
                for (int i = startingindex; i < nextIndex; i++)
                {
                    string name = reader.GetName(i);
                    for (int j = 0; j < columnsCount; j++)
                    {
                        if (columns.ColumnsNames[j].Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            order[j + startingindex] = i;
                            break;
                        }
                    }
                }

                startingindex += columnsCount;
            }

            if (startingindex != fieldCount)
                throw new InvalidOperationException("columns count different than expected");

            return order;
        }

        public static IEnumerable<T> Fetch<T>(this FaaastCommand command)
        {
            return Read<T>(command);
        }

        public static T FirstOrDefault<T>(this FaaastCommand command)
        {
            foreach (var item in Read<T>(command))
            {
                return item;
            }

            return default;
        }

        public static T Single<T>(this FaaastCommand command)
        {
            int row = 0;
            T result = default;
            foreach (var item in Read<T>(command))
            {
                result = item;
                row++;
                if (row > 1)
                    throw new InvalidOperationException("Seqence contains more than one element");
            }

            return result;
        }

        public static IEnumerable<object[]> Fetch(this FaaastCommand command, params Type[] types)
        {
            return Read(command, types);
        }

        public static object[] FirstOrDefault(this FaaastCommand command, params Type[] types)
        {
            foreach (var item in Read(command, types))
            {
                return item;
            }

            return default;
        }

        public static object[] Single(this FaaastCommand command, params Type[] types)
        {
            int row = 0;
            object[] result = default;
            foreach (var item in Read(command, types))
            {
                result = item;
                row++;
                if (row > 1)
                    throw new InvalidOperationException("Seqence contains more than one element");
            }

            return result;
        }

        public static IAsyncEnumerable<T> FetchAsync<T>(this FaaastCommand command)
        {
            return ReadAsync<T>(command);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this FaaastCommand command)
        {
            await foreach (var item in ReadAsync<T>(command).ConfigureAwait(false))
            {
                return item;
            }

            return default;
        }

        public static async Task<T> SingleAsync<T>(this FaaastCommand command)
        {
            int row = 0;
            T result = default;
            await foreach (var item in ReadAsync<T>(command).ConfigureAwait(false))
            {
                result = item;
                row++;
                if (row > 1)
                    throw new InvalidOperationException("Seqence contains more than one element");
            }

            return result;
        }

        public static IAsyncEnumerable<object[]> FetchAsync(this FaaastCommand command, params Type[] types)
        {
            return ReadAsync(command, types);
        }

        public static async Task<object[]> FirstOrDefaultAsync(this FaaastCommand command, params Type[] types)
        {
            await foreach (var item in ReadAsync(command, types).ConfigureAwait(false))
            {
                return item;
            }

            return default;
        }

        public static async Task<object[]> SingleAsync(this FaaastCommand command, params Type[] types)
        {
            int row = 0;
            object[] result = default;
            await foreach (var item in ReadAsync(command, types).ConfigureAwait(false))
            {
                result = item;
                row++;
                if (row > 1)
                    throw new InvalidOperationException("Seqence contains more than one element");
            }

            return result;
        }
    }
}
