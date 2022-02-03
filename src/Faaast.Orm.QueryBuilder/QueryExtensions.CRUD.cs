using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Faaast.Orm.Mapping;
using Faaast.Orm.Reader;
namespace Faaast.Orm
{
    public static partial class QueryExtensions
    {
        public static TableMapping Mapping<TClass>(this FaaastQueryDb db)
        {
            var mapping = db.Mappings.Value;
            return mapping.TypeToMapping[typeof(TClass)];
        }

        public static Task<int> DeleteAsync<T>(this FaaastQueryDb db, T record, FaaastCommand? command = null)
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
            var cmd = command ?? db.Query(null);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = compiledQuery.Sql;
            cmd.Parameters = compiledQuery.Parameters;
            return cmd.ExecuteAsync();
        }

        public static Task<int> UpdateAsync<T>(this FaaastQueryDb db, T record, FaaastCommand? command = null)

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
            var cmd = command ?? db.Query(null);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = compiledQuery.Sql;
            cmd.Parameters = compiledQuery.Parameters;
            return cmd.ExecuteAsync();
        }

        public static async Task<int> InsertAsync<T>(this FaaastQueryDb db, T record, FaaastCommand? command = null)
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
            var cmd = command ?? db.Query(null);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = compiledQuery.Sql;
            cmd.Parameters = compiledQuery.Parameters;

            if (identityColumn == null)
            {
                return await cmd.ExecuteAsync();
            }
            else
            {
                if (cmd.HandleConnection)
                {
                    cmd.Connection.Open();
                }

                using (IDbCommand dbCommand = await command.Value.PrepareAsync())
                {
                    var dbReader = dbCommand.ExecuteReader(cmd.CommandBehavior);
                    if(dbReader.Read())
                    {
                        object id = dbReader.GetValue(0);
                        var convertedId = Convert.ChangeType(id, identityColumn.Property.Type);
                        identityColumn.Property.Write(record, convertedId);
                    }
                }

                if (cmd.HandleConnection)
                {
                    cmd.Connection.Close();
                    cmd.Connection.Dispose();
                }
            }

            return 1;
        }
    }
}
