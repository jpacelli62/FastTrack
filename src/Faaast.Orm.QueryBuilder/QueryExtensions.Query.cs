using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Faaast.Orm.Reader;

namespace Faaast.Orm
{
    public static partial class QueryExtensions
    {
        public static Task<TClass> FirstOrDefaultAsync<TClass>(this FaaastQuery query,
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var command = query.CreateCommand(connection, transaction, commandTimeout, cancellationToken);
            return command.FirstOrDefaultAsync<TClass>();
        }

        public static Task<TClass> FirstOrDefaultAsync<TClass>(this FaaastQuery<TClass> query,
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var command = query.CreateCommand(connection, transaction, commandTimeout, cancellationToken);
            return command.FirstOrDefaultAsync<TClass>();
        }

        public static FaaastCommand CreateCommand(this FaaastQuery query,
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var compiledQuery = query.Compile();
            return query.Db.Query(
                compiledQuery.Sql,
                compiledQuery.Parameters,
                connection,
                transaction,
                commandTimeout,
                CommandType.Text,
                cancellationToken
                );
        }

        public static async Task<ICollection<TClass>> ToListAsync<TClass>(this FaaastQuery query,
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var command = query.CreateCommand(connection, transaction, commandTimeout, cancellationToken);
            var result = new List<TClass>();
            await foreach (var row in command.FetchAsync<TClass>())
            {
                result.Add(row);
            }

            return result;
        }

        public static async Task<ICollection<TClass>> ToListAsync<TClass>(this FaaastQuery<TClass> query,
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var command = query.CreateCommand(connection, transaction, commandTimeout, cancellationToken);
            var result = new List<TClass>();
            await foreach (var row in command.FetchAsync<TClass>())
            {
                result.Add(row);
            }

            return result;
        }
    }
}
