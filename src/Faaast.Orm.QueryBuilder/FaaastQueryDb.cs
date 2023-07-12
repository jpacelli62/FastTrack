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

namespace Faaast.Orm
{
    public abstract class FaaastQueryDb : FaaastDb
    {
        public abstract SqlKata.Compilers.Compiler Compiler { get; }

        protected FaaastQueryDb(IServiceProvider services) : base(services)
        {
        }

        public TableAlias<T> Alias<T>(string alias = null)
        {
            var mapping = this.Mapping<T>();
            return mapping != null ? new TableAlias<T>(mapping, alias) : null;
        }

        public SqlKata.Query Sql => new FaaastQuery(this);

        public virtual CompiledQuery Compile(SqlKata.Query query)
        {
            var result = this.Compiler.Compile(query);
            return new CompiledQuery(result.Sql, result.NamedBindings);
        }

        protected FaaastQuery CreateGetAllQuery<T>()
        {
            var mapping = this.Mapping<T>();
            var sql = new FaaastQuery(this, mapping.Table.Name);
            sql.Select(mapping.Table.Columns.Select(x => x.Name).ToArray());
            return sql;
        }

        public async Task<ICollection<T>> GetAllAsync<T>(DbConnection connection = null) =>
            await this.CreateGetAllQuery<T>().ToListAsync<T>(connection);

        public ICollection<T> GetAll<T>(DbConnection connection = null) =>
            this.CreateGetAllQuery<T>().ToList<T>(connection);

        protected FaaastQuery BuildDeleteQuery<T>(T record)
        {
            var mapping = this.Mapping<T>();
            var sql = new FaaastQuery(this, mapping.Table.Name);

            var pk = mapping.Table.PrimaryKeyColumns();
            foreach (var column in pk)
            {
                var value = ConvertValue(record, mapping, column);
                sql.Where(column.Name, value);
            }

            sql.AsDelete();
            return sql;
        }

        public async Task<int> DeleteAsync<T>(T record, DbConnection connection = null)
        {
            var sql = this.BuildDeleteQuery(record);
            await using var command = await sql.CreateCommandAsync(connection);
            var result = await command.ExecuteNonQueryAsync();
            return result;
        }

        public int Delete<T>(T record, DbConnection connection = null)
        {
            var sql = this.BuildDeleteQuery(record);
            using var command = sql.CreateCommand(connection);
            var result = command.ExecuteNonQuery();
            return result;
        }

        protected FaaastQuery BuildUpdateQuery<T>(T record)
        {
            var mapping = this.Mapping<T>();
            var sql = new FaaastQuery(this, mapping.Table.Name);

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

        public async Task<int> UpdateAsync<T>(T record, DbConnection connection = null)
        {
            var sql = this.BuildUpdateQuery(record);
            if (sql == null)
            {
                return 0;
            }

            await using var command = await sql.CreateCommandAsync(connection);
            var result = await command.ExecuteNonQueryAsync();
            return result;
        }

        public int Update<T>(T record, DbConnection connection = null)
        {
            var sql = this.BuildUpdateQuery(record);
            if (sql == null)
            {
                return 0;
            }

            using var command = sql.CreateCommand(connection);
            var result = command.ExecuteNonQuery();
            return result;
        }

        public async Task<int> InsertAsync<T>(T record, DbConnection connection = null)
        {
            var mapping = this.Mapping<T>();
            SqlKata.Query sql = new FaaastQuery(this, mapping.Table.Name);

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
                    lastId = this.Compiler.EngineCode switch
                    {
                        EngineCodes.MySql => "SELECT last_insert_id() as Id",
                        EngineCodes.SqlServer => "SELECT scope_identity() as Id",
                        EngineCodes.Sqlite => "select last_insert_rowid() as id",
                        EngineCodes.PostgreSql => "SELECT lastval() AS id",
                        _ => throw new NotImplementedException()
                    };
                }

                var sqlQuery = $"INSERT INTO {this.Compiler.Wrap(mapping.Table.Name)} DEFAULT VALUES;{lastId}";
                command = await this.CreateCommandAsync(sqlQuery, connection);
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
     
        public int Insert<T>(T record, DbConnection connection = null)
        {
            var mapping = this.Mapping<T>();
            SqlKata.Query sql = new FaaastQuery(this, mapping.Table.Name);

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
                    lastId = this.Compiler.EngineCode switch
                    {
                        EngineCodes.MySql => "SELECT last_insert_id() as Id",
                        EngineCodes.SqlServer => "SELECT scope_identity() as Id",
                        EngineCodes.Sqlite => "select last_insert_rowid() as id",
                        EngineCodes.PostgreSql => "SELECT lastval() AS id",
                        _ => throw new NotImplementedException()
                    };
                }

                var sqlQuery = $"INSERT INTO {this.Compiler.Wrap(mapping.Table.Name)} DEFAULT VALUES;{lastId}";
                command = this.CreateCommand(sqlQuery, connection);
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
    }
}
