using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Faaast.Orm.Mapping;

namespace Faaast.Orm
{
    public static partial class QueryExtensions
    {
        public static TableMapping Mapping<TClass>(this FaaastQueryDb db)
        {
            var mapping = db.Mappings.Value;
            return mapping.TypeToMapping[typeof(TClass)];
        }

        public static Task<int> DeleteAsync<T>(this FaaastQueryDb db, T record,
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)

        {
            var mapping = db.Mapping<T>();
            var pk = mapping.Table.PrimaryKeyColumns();
            var where = new Dictionary<string, object>();
            foreach (var column in pk)
            {
                where.Add(column.Name, mapping.ColumnToProperty[column].Read(record));
            }

            var query = db.From<T>().Where(where).AsDelete();
            var compiledQuery = db.Compile(query);
            var command = db.Query(
                compiledQuery.Sql,
                compiledQuery.Parameters,
                connection,
                transaction,
                commandTimeout,
                CommandType.Text,
                cancellationToken
                );

            return command.ExecuteAsync();
        }

        public static Task<int> UpdateAsync<T>(this FaaastQueryDb db, T record,
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)

        {
            var mapping = db.Mapping<T>();
            var where = new Dictionary<string, object>();
            var update = new Dictionary<string, object>();
            foreach (var column in mapping.ColumnMappings)
            {
                if (column.Column.PrimaryKey)
                {
                    where.Add(column.Column.Name, column.Property.Read(record));
                }
                else
                {
                    if (!column.Column.Identity)
                    {
                        update.Add(column.Column.Name, column.Property.Read(record));
                    }
                }
            }

            var query = db.From<T>().Where(where).AsUpdate(update);
            var compiledQuery = db.Compile(query);
            var command = db.Query(
                compiledQuery.Sql,
                compiledQuery.Parameters,
                connection,
                transaction,
                commandTimeout,
                CommandType.Text,
                cancellationToken);
            return command.ExecuteAsync();
        }

        public static async Task<int> InsertAsync<T>(this FaaastQueryDb db, T record, DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var mapping = db.Mapping<T>();
            var insert = new Dictionary<string, object>();
            ColumnMapping identityColumn = null;

            foreach (var column in mapping.ColumnMappings)
            {
                if (column.Column.Identity)
                {
                    identityColumn = column;
                }
                else if (!column.Column.Computed)
                {
                    insert.Add(column.Column.Name, column.Property.Read(record));
                }
            }

            var query = db.From<T>().AsInsert(insert, identityColumn != null);
            var compiledQuery = db.Compile(query);
            var command = db.Query(
                compiledQuery.Sql,
                compiledQuery.Parameters,
                connection,
                transaction,
                commandTimeout,
                CommandType.Text,
                cancellationToken);
            if (identityColumn == null)
            {
                return await command.ExecuteAsync();
            }
            else
            {
                object id = await command.SingleAsync(identityColumn.Property.Type);
                identityColumn.Property.Write(record, id);
            }

            return 1;
        }
    }
}
