using Faaast.DatabaseModel;
using Faaast.Metadata;
using Faaast.Orm.Reader;
using Faaast.Reader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                    dbCommand.Prepare();
                    var dbReader = dbCommand.ExecuteReader(command.CommandBehavior);
                    CompositeReader composite = new CompositeReader(command, dbReader, typeof(T));

                    while (dbReader.Read())
                    {
                        yield return (T)composite.Read(dbReader)[0];
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
                    var dbReader = dbCommand.ExecuteReader(command.CommandBehavior);
                    CompositeReader composite = new CompositeReader(command, dbReader, types);

                    while (dbReader.Read())
                    {
                        yield return composite.Read(dbReader);
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
                    await dbCommand.TryPrepareAsync(command.CancellationToken).ConfigureAwait(false);
                    var dbReader = await dbCommand.ExecuteReaderAsync(command.CommandBehavior, command.CancellationToken).ConfigureAwait(false);
                    CompositeReader composite = new CompositeReader(command, dbReader, typeof(T));

                    while (await dbReader.ReadAsync(command.CancellationToken).ConfigureAwait(false))
                    {
                        yield return (T)composite.Read(dbReader)[0];
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
                    var dbReader = await dbCommand.ExecuteReaderAsync(command.CommandBehavior, command.CancellationToken).ConfigureAwait(false);
                    CompositeReader composite = new CompositeReader(command, dbReader, types);

                    while (await dbReader.ReadAsync(command.CancellationToken).ConfigureAwait(false))
                    {
                        yield return composite.Read(dbReader);
                    }
                }

                await TryCloseAsync(command.Connection, command.CancellationToken).ConfigureAwait(false);
            }
        }

        //internal static Dictionary<string, Action> ResolveColumnOrder(IDataReader reader, params ObjectReader[] objects)
        //{
        //    var readers = new Dictionary<string, Action>();
        //    if (objects.Length == 1)
        //    {
        //        var objReader = objects[0];
        //        var columnNames = objReader.ColumnsNames;
        //        for (int readerIndex = 0; readerIndex < reader.FieldCount; readerIndex++)
        //        {
        //            string dataName = reader.GetName(readerIndex);
        //            for (int j = 0; j < columnNames.Length; j++)
        //            {
        //                if (columnNames[j].Equals(dataName, StringComparison.OrdinalIgnoreCase))
        //                {
        //                    readers.Add(dataName, () => objReader.Columns[j].Read(reader, readerIndex, objReader.Instance));
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        throw new NotImplementedException();
        //    }
        //    return readers;



        //    //for (int i = 0; i < objects.Length; i++)
        //    //{
        //    //    var c = objects[i];
        //    //    ResolveSingleObject(
        //    //        startingindex, 
        //    //        objects.Length == 1 ? 0 : startingindex, 
        //    //        i == objects.Length - 1 ? reader.FieldCount : c.ColumnsNames.Length,
        //    //        count, 
        //    //        IDataReader reader, string[] columnNames, int[] order)
        //    //}
        //    //foreach (var columns in objects)
        //    //{
        //    //int columnsCount = columns.ColumnsNames.Length;
        //    //int nextIndex = startingindex + columnsCount;
        //    //for (int i = startingindex; i < nextIndex; i++)
        //    //{
        //    //    string name = reader.GetName(i);
        //    //    for (int j = 0; j < columnsCount; j++)
        //    //    {
        //    //        if (columns.ColumnsNames[j].Equals(name, StringComparison.OrdinalIgnoreCase))
        //    //        {
        //    //            order[j + startingindex] = i;
        //    //            break;
        //    //        }
        //    //    }
        //    //}

        //    //    startingindex += columnsCount;
        //    //}

        //    //return order;
        //}

        //internal static void ResolveSingleObject(int start, int count, IDataReader reader, string[] columnNames, int[] order)
        //{
        //    int nextIndex = start + count;
        //    for (int i = start; i < nextIndex; i++)
        //    {
        //        string name = reader.GetName(i);
        //        for (int j = 0; j < columnNames.Length; j++)
        //        {
        //            if (columnNames[j].Equals(name, StringComparison.OrdinalIgnoreCase))
        //            {
        //                order[j + start] = i;
        //                break;
        //            }
        //        }
        //    }
        //}

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
