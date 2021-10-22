using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Faaast.Orm;
using SqlKata;

namespace Faaast.Orm
{
    public static class QueryExtensions
    {
        public static TableMapping Mapping<TClass>(this FaaastDb db)
        {
            var mapping = db.GetMapping();
            return mapping.TypeToMapping[typeof(TClass)];
        }

        public static Task<T> GetAsync<T>(this FaaastQueryDb db, object id,
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var mapping = db.Mapping<T>();
            string tableName = mapping.Table.Name;

            var query = new Query(tableName);
            foreach (var column in mapping.ColumnMappings)
            {
                query.AddComponent("select", new SqlKata.Column
                {
                    Name = column.Column.Name
                });
            }

            if (id is IEnumerable<ColumnClause> clauses)
            {
                foreach (var clause in clauses)
                {
                    query.Where(clause.ColumnName(mapping), clause.Operator, clause.Value);
                }
            }
            else
            {
                var pk = mapping.Table.PrimaryKeyColumns().Single();
                query.Where(pk.Name, id);
            }

            var compiledQuery = db.Compiler.Compile(query);
            var command = db.Query(
                compiledQuery.Sql.Replace("ariane].[", "ariane."),
                compiledQuery.NamedBindings,
                connection,
                transaction,
                commandTimeout,
                CommandType.Text,
                cancellationToken
                );

            return command.FirstOrDefaultAsync<T>();
        }


        public static async Task<ICollection<T>> AllAsync<T>(this FaaastQueryDb db, 
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var mapping = db.Mapping<T>();
            string tableName = mapping.Table.Name;

            var query = new Query(tableName);
            foreach (var column in mapping.ColumnMappings)
            {
                query.AddComponent("select", new SqlKata.Column
                {
                    Name = column.Column.Name
                });
            }
            var compiledQuery = db.Compiler.Compile(query);
            var command = db.Query(
                compiledQuery.Sql.Replace("ariane].[", "ariane."), 
                compiledQuery.NamedBindings,
                connection,
                transaction,
                commandTimeout,
                CommandType.Text,
                cancellationToken
                );

            List<T> result = new List<T>();
            await foreach (var row in command.FetchAsync<T>())
            {
                result.Add(row);
            }

            return result;
        }

        public static Task<int> DeleteAsync<T>(this FaaastQueryDb db, T record, 
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)

        {
            var mapping = db.Mapping<T>();
            string tableName = mapping.Table.Name;
            var pk = mapping.Table.PrimaryKeyColumns();
            var where = new Dictionary<string, object>();
            foreach (var column in pk)
            {
                where.Add(column.Name, mapping.ColumnToProperty[column].Read(record));
            }

            var query = new Query(tableName).Where(where).AsDelete();
            var compiledQuery = db.Compiler.Compile(query);
            var command = db.Query(
                compiledQuery.Sql.Replace("ariane].[", "ariane."), 
                compiledQuery.NamedBindings,
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
            string tableName = mapping.Table.Name;
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

            var query = new Query(tableName).Where(where).AsUpdate(update);
            var compiledQuery = db.Compiler.Compile(query);
            var command = db.Query(
                compiledQuery.Sql.Replace("ariane].[", "ariane."), 
                compiledQuery.NamedBindings,
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
            string tableName = mapping.Table.Name;
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

            var query = new Query(tableName).AsInsert(insert, identityColumn != null);
            var compiledQuery = db.Compiler.Compile(query);
            var command = db.Query(
                compiledQuery.Sql.Replace("ariane].[", "ariane."), 
                compiledQuery.NamedBindings, 
                connection,
                transaction,
                commandTimeout,
                CommandType.Text,
                cancellationToken);
            if (identityColumn == null)
                return await command.ExecuteAsync();
            else
            {
                object id = await command.SingleAsync(identityColumn.Property.Type);
                identityColumn.Property.Write(record, id);
            }

            return 1;
        }
    }
}
