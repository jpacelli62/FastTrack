using System;
using System.Collections.Generic;
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
            await command.ExecuteReaderAsync(stuff);
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

        public static async Task<T> FirstOrDefaultAsync<T>(this Query query, DbConnection dbConnection = null)
        {
            using var command = await query.CreateCommandAsync(dbConnection);
            return await command.FirstOrDefaultAsync<T>();
        }
    }
}
