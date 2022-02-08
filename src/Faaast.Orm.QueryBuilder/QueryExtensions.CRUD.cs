﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Faaast.Orm.Mapping;
using Faaast.Orm.Model;
using Faaast.Orm.Reader;
namespace Faaast.Orm
{
    public static partial class QueryExtensions
    {
        public static async Task<int> DeleteAsync<T>(this FaaastQueryDb db, T record, AsyncFaaastCommand command = null)
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
            var cmd = command ?? await db.CreateCommandAsync(compiledQuery.Sql, compiledQuery.Parameters);
            return await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<int> UpdateAsync<T>(this FaaastQueryDb db, T record, AsyncFaaastCommand command = null)

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
            var cmd = command ?? await db.CreateCommandAsync(compiledQuery.Sql, compiledQuery.Parameters);
            return await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<int> InsertAsync<T>(this FaaastQueryDb db, T record, AsyncFaaastCommand command = null)
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
            var cmd = command ?? await db.CreateCommandAsync(compiledQuery.Sql, compiledQuery.Parameters);

            if (identityColumn == null)
            {
                return await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                var reader = await cmd.ExecuteReaderAsync();
                if(await reader.ReadAsync())
                {
                    var convertedId = Convert.ChangeType(reader.Buffer[0], identityColumn.Property.Type);
                    identityColumn.Property.Write(record, convertedId);
                }
            }

            return 1;
        }
    }
}
