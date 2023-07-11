using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Faaast.Orm.Reader;
using SqlKata;

namespace Faaast.Orm
{
    public static class CommandAsyncExtensions
    {
        public static async Task<FaaastCommand> CreateCommandAsync(this Query query, DbConnection dbConnection = null)
        {
            var q = (FaaastQuery)query;
            var compiledQuery = q.Db.Compile(q);
            return await q.Db.CreateCommandAsync(compiledQuery.Sql, compiledQuery.Parameters, dbConnection);
        }

        public static async Task ExecuteReaderAsync(this Query query, Func<AsyncFaaastRowReader, Task> stuff, DbConnection dbConnection = null)
        {
            using var command = await CreateCommandAsync(query, dbConnection);
            await ExecuteReaderAsync(command, stuff);
        }

        public static async Task ExecuteReaderAsync(this Task<FaaastCommand> commandTask, Func<AsyncFaaastRowReader, Task> stuff)
        {
            using var command = await commandTask;
            await ExecuteReaderAsync(command, stuff);
        }

        public static async Task ExecuteReaderAsync(this FaaastCommand command, Func<AsyncFaaastRowReader, Task> stuff)
        {
            await using var reader = await command.ExecuteReaderAsync();
            await stuff(reader);
        }

        public static async Task ExecuteNonQueryAsync(this Task<FaaastCommand> commandTask, DbConnection dbConnection = null)
        {
            using var command = await commandTask;
            await ExecuteNonQueryAsync(command, dbConnection);
        }

        public static async Task ExecuteNonQueryAsync(this FaaastCommand command, DbConnection dbConnection = null)
        {
            await command.ExecuteNonQueryAsync();
        }

        public static async Task ExecuteNonQueryAsync(this Query query, DbConnection dbConnection = null)
        {
            using var command = await CreateCommandAsync(query, dbConnection);
            await command.ExecuteNonQueryAsync();
        }

        public static async Task<ICollection<T>> ToListAsync<T>(this Query query, DbConnection dbConnection = null)
        {
            using var command = await query.CreateCommandAsync(dbConnection);
            return await command.ToListAsync<T>();
        }

        public static async Task<ICollection<T>> ToListAsync<T>(this Task<FaaastCommand> command)
        {
            return await ToListAsync<T>(await command);
        }

        public static async Task<ICollection<T>> ToListAsync<T>(this FaaastCommand command)
        {
            var result = new List<T>();
            await ExecuteReaderAsync(command, async reader =>
            {
                var tReader = reader.AddReader<T>();
                while (await reader.ReadAsync())
                {
                    result.Add(tReader.Value);
                }
            });

            return result;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Query query, DbConnection dbConnection = null)
        {
            using var command = await query.CreateCommandAsync(dbConnection);
            return await FirstOrDefaultAsync<T>(command);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Task<FaaastCommand> commandTask)
        {
            using var command = await commandTask;
            return await FirstOrDefaultAsync<T>(command);
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this FaaastCommand command)
        {
            T result = default;
            await ExecuteReaderAsync(command, async reader =>
            {
                var tReader = reader.AddReader<T>();
                if (await reader.ReadAsync())
                {
                    result = tReader.Value;
                }
            });

            return result;
        }
    }
}
