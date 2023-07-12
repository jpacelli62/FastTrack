using System;
using System.Collections.Generic;
using System.Data.Common;
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
        private static object ConvertValue<T>(T record, TableMapping mapping, Column column)
        {
            var property = mapping.ColumnToProperty[column];
            var value = property.Read(record);

            var converterType = column.ConverterType;
            var converterInstance = column.ConverterInstance;

            if (converterInstance is null && converterType != null)
            {
                converterInstance = (IValueConverter)Activator.CreateInstance(converterType);
                column.ConverterInstance = converterInstance;
            }

            if (converterInstance != null)
            {
                value = converterInstance.ToDb(value, property.Type);
            }

            return value;
        }
        public static Task<ICollection<T>> GetAllAsync<T>(this FaaastQueryDb db)=> GetAllAsync<T>(db, null);

        public static async Task<ICollection<T>> GetAllAsync<T>(this FaaastQueryDb db, DbConnection connection)
        {
            var mapping = db.Mapping<T>();
            var sql = new FaaastQuery(db, mapping.Table.Name);
            sql.Select(mapping.Table.Columns.Select(x => x.Name).ToArray());
            return await sql.ToListAsync<T>(connection);
        }
        public static ICollection<T> GetAll<T>(this FaaastQueryDb db, DbConnection connection)
        {
            var mapping = db.Mapping<T>();
            var sql = new FaaastQuery(db, mapping.Table.Name);
            sql.Select(mapping.Table.Columns.Select(x => x.Name).ToArray());
            return sql.ToList<T>(connection);
        }

        private static FaaastQuery BuildDeleteQuery<T>(FaaastQueryDb db, T record)
        {
            var mapping = db.Mapping<T>();
            var sql = new FaaastQuery(db, mapping.Table.Name);

            var pk = mapping.Table.PrimaryKeyColumns();
            foreach (var column in pk)
            {
                var value = ConvertValue(record, mapping, column);
                sql.Where(column.Name, value);
            }

            sql.AsDelete();
            return sql;
        }

        public static Task<int> DeleteAsync<T>(this FaaastQueryDb db, T record) => DeleteAsync(db, record, null);

        public static async Task<int> DeleteAsync<T>(this FaaastQueryDb db, T record, DbConnection connection)
        {
            var sql = BuildDeleteQuery(db, record);
            await using var command = await sql.CreateCommandAsync(connection);
            var result = await command.ExecuteNonQueryAsync();
            return result;
        }

        public static int Delete<T>(this FaaastQueryDb db, T record) => Delete(db, record, null);

        public static int Delete<T>(this FaaastQueryDb db, T record, DbConnection connection)
        {
            var sql = BuildDeleteQuery(db, record);
            using var command = sql.CreateCommand(connection);
            var result = command.ExecuteNonQuery();
            return result;
        }

        private static FaaastQuery BuildUpdateQuery<T>(FaaastQueryDb db, T record)
        {
            var mapping = db.Mapping<T>();
            var sql = new FaaastQuery(db, mapping.Table.Name);

            var update = new Dictionary<string, object>();
            foreach (var (column, value) in from column in mapping.ColumnMappings
                                            let value = ConvertValue(record, mapping, column.Column)
                                            select (column, value))
            {
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
                return null;
            }

            sql.AsUpdate(update);
            return sql;
        }

        public static Task<int> UpdateAsync<T>(this FaaastQueryDb db, T record) => UpdateAsync(db, record, null);

        public static async Task<int> UpdateAsync<T>(this FaaastQueryDb db, T record, DbConnection connection)
        {
            var sql = BuildUpdateQuery(db, record);
            if(sql == null)
            {
                return 0;
            }

            await using var command = await sql.CreateCommandAsync(connection);
            var result = await command.ExecuteNonQueryAsync();
            return result;
        }

        public static int Update<T>(this FaaastQueryDb db, T record) => Update(db, record, null);

        public static int Update<T>(this FaaastQueryDb db, T record, DbConnection connection)
        {
            var sql = BuildUpdateQuery(db, record);
            if (sql == null)
            {
                return 0;
            }

            using var command = sql.CreateCommand(connection);
            var result = command.ExecuteNonQuery();
            return result;
        }

        public static Task<int> InsertAsync<T>(this FaaastQueryDb db, T record) => InsertAsync(db, record, null);

        public static async Task<int> InsertAsync<T>(this FaaastQueryDb db, T record, DbConnection connection)
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
                    var value = ConvertValue(record, mapping, column.Column);
                    insert.Add(column.Column.Name, value);
                }
            }

            FaaastCommand command = null;
            if (!insert.Any())
            {
                var lastId = string.Empty;
                if (identityColumn != null)
                {
                    lastId = db.Compiler.EngineCode switch
                    {
                        EngineCodes.MySql => "SELECT last_insert_id() as Id",
                        EngineCodes.SqlServer => "SELECT scope_identity() as Id",
                        EngineCodes.Sqlite => "select last_insert_rowid() as id",
                        EngineCodes.PostgreSql => "SELECT lastval() AS id",
                        _ => throw new NotImplementedException()
                    };
                }

                var sqlQuery = $"INSERT INTO {db.Compiler.Wrap(mapping.Table.Name)} DEFAULT VALUES;{lastId}";
                command = await db.CreateCommandAsync(sqlQuery, connection);
            }
            else
            {
                sql.AsInsert(insert, identityColumn != null);
                command = await sql.CreateCommandAsync(connection);
            }

            if (identityColumn == null)
            {
                await using (command)
                {
                    return await command.ExecuteNonQueryAsync();
                }
            }
            else
            {
                object value = null;
                await using (command)
                {
                    await command.ExecuteReaderAsync(async reader =>
                    {
                        var tReader = reader.AddReader(identityColumn.Property.Type);
                        if (await reader.ReadAsync())
                        {
                            value = tReader.RawValue;
                        }
                    });

                    var converterType = identityColumn.Column.ConverterType;
                    var converterInstance = identityColumn.Column.ConverterInstance;

                    if (converterInstance is null && converterType != null)
                    {
                        converterInstance = (IValueConverter)Activator.CreateInstance(converterType);
                        identityColumn.Column.ConverterInstance = converterInstance;
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

        public static int Insert<T>(this FaaastQueryDb db, T record) => Insert(db, record, null);

        public static int Insert<T>(this FaaastQueryDb db, T record, DbConnection connection)
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
                    var value = ConvertValue(record, mapping, column.Column);
                    insert.Add(column.Column.Name, value);
                }
            }

            FaaastCommand command = null;
            if (!insert.Any())
            {
                var lastId = string.Empty;
                if (identityColumn != null)
                {
                    lastId = db.Compiler.EngineCode switch
                    {
                        EngineCodes.MySql => "SELECT last_insert_id() as Id",
                        EngineCodes.SqlServer => "SELECT scope_identity() as Id",
                        EngineCodes.Sqlite => "select last_insert_rowid() as id",
                        EngineCodes.PostgreSql => "SELECT lastval() AS id",
                        _ => throw new NotImplementedException()
                    };
                }

                var sqlQuery = $"INSERT INTO {db.Compiler.Wrap(mapping.Table.Name)} DEFAULT VALUES;{lastId}";
                command = db.CreateCommand(sqlQuery, connection);
            }
            else
            {
                sql.AsInsert(insert, identityColumn != null);
                command = sql.CreateCommand(connection);
            }

            if (identityColumn == null)
            {
                using (command)
                {
                    return command.ExecuteNonQuery();
                }
            }
            else
            {
                object value = null;
                using (command)
                {
                    command.ExecuteReader(reader =>
                    {
                        var tReader = reader.AddReader(identityColumn.Property.Type);
                        if (reader.Read())
                        {
                            value = tReader.RawValue;
                        }
                    });

                    var converterType = identityColumn.Column.ConverterType;
                    var converterInstance = identityColumn.Column.ConverterInstance;

                    if (converterInstance is null && converterType != null)
                    {
                        converterInstance = (IValueConverter)Activator.CreateInstance(converterType);
                        identityColumn.Column.ConverterInstance = converterInstance;
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
