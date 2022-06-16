using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Faaast.Orm.Converters;
using Faaast.Orm.Mapping;
using Faaast.Orm.Model;
using Faaast.Orm.Reader;
using SqlKata.Compilers;
using Kata = SqlKata;

namespace Faaast.Orm
{
    public static partial class QueryExtensions
    {
        private static object ConvertValue<T>(FaaastQueryDb db, T record, TableMapping mapping, Column column)
        {
            var property = mapping.ColumnToProperty[column];
            var value = property.Read(record);

            var converterType = column.Get(DbMeta.Converter);
            var converterInstance = column.Get(DbMeta.ConverterInstance);

            if (converterInstance is null && converterType != null)
            {
                converterInstance = (IValueConverter)Activator.CreateInstance(converterType);
                column.Set(DbMeta.ConverterInstance, converterInstance);
            }

            if (converterInstance != null)
            {
                value = converterInstance.ToDb(value, property.Type);
            }

            return value;
        }

        public static async Task<ICollection<T>> GetAllAsync<T>(this FaaastQueryDb db)
        {
            var mapping = db.Mapping<T>();
            var sql = new FaaastQuery(db, mapping.Table.Name);
            sql.Select(mapping.Table.Columns.Select(x => x.Name).ToArray());
            return await sql.ToListAsync<T>();
        }

        public static async Task<int> DeleteAsync<T>(this FaaastQueryDb db, T record)
        {
            var mapping = db.Mapping<T>();
            var sql = new FaaastQuery(db, mapping.Table.Name);

            var pk = mapping.Table.PrimaryKeyColumns();
            foreach (var column in pk)
            {
                var value = ConvertValue(db, record, mapping, column);
                sql.Where(column.Name, value);
            }

            sql.AsDelete();

            using var command = await sql.CreateCommandAsync();
            var result = await command.ExecuteNonQueryAsync();
            return result;
        }

        public static async Task<int> UpdateAsync<T>(this FaaastQueryDb db, T record)
        {
            var mapping = db.Mapping<T>();
            var sql = new FaaastQuery(db, mapping.Table.Name);

            var update = new Dictionary<string, object>();
            foreach (var column in mapping.ColumnMappings)
            {
                var value = ConvertValue(db, record, mapping, column.Column);
                if (column.Column.PrimaryKey)
                {
                    sql.Where(column.Column.Name, value);
                }
                else
                {
                    if (!column.Column.Identity)
                    {
                        update.Add(column.Column.Name, value);
                    }
                }
            }

            if (!update.Any())
            {
                return 0;
            }

            sql.AsUpdate(update);

            using var command = await sql.CreateCommandAsync();
            var result = await command.ExecuteNonQueryAsync();
            return result;
        }

        public static async Task<int> InsertAsync<T>(this FaaastQueryDb db, T record)
        {
            var mapping = db.Mapping<T>();
            Kata.Query sql = new FaaastQuery(db, mapping.Table.Name);

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
                    var value = ConvertValue(db, record, mapping, column.Column);
                    insert.Add(column.Column.Name, value);
                }
            }

            AsyncFaaastCommand command = null;
            if (!insert.Any())
            {
                string lastId = string.Empty;
                if (identityColumn != null)
                {
                    lastId = db.Compiler.EngineCode switch
                    {
                        EngineCodes.MySql => "SELECT last_insert_id() as Id",
                        EngineCodes.SqlServer => "SELECT scope_identity() as Id",
                        EngineCodes.Sqlite => "select last_insert_rowid() as id",
                        EngineCodes.PostgreSql => "SELECT lastval() AS id"
                    };
                }

                var sqlQuery = $"INSERT INTO {db.Compiler.Wrap(mapping.Table.Name)} DEFAULT VALUES;{lastId}";
                command = await db.CreateCommandAsync(sqlQuery);
            }
            else
            {
                sql.AsInsert(insert, identityColumn != null);
                command = await sql.CreateCommandAsync();
            }

            if (identityColumn == null)
            {
                using (command)
                {
                    return await command.ExecuteNonQueryAsync();
                }
            }
            else
            {
                object value = null;
                using (command)
                {
                    await command.ExecuteReaderAsync(async reader =>
                    {
                        var tReader = reader.AddReader(identityColumn.Property.Type);
                        if (await reader.ReadAsync())
                        {
                            value = tReader.RawValue;
                        }
                    });

                    var converterType = identityColumn.Column.Get(DbMeta.Converter);
                    var converterInstance = identityColumn.Column.Get(DbMeta.ConverterInstance);

                    if (converterInstance is null && converterType != null)
                    {
                        converterInstance = (IValueConverter)Activator.CreateInstance(converterType);
                        identityColumn.Column.Set(DbMeta.ConverterInstance, converterInstance);
                    }

                    if (converterInstance != null)
                    {
                        value = converterInstance.FromDb(value, identityColumn.Property.Type);
                    }

                    identityColumn.Property.Write(record, value);
                }
            }

            return 1;
        }
    }
}
