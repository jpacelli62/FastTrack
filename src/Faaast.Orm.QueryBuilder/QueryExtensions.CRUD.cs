using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Faaast.Orm;
using Faaast.Orm.Converters;
using Faaast.Orm.Mapping;
using Faaast.Orm.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Faaast.Orm
{
    public static partial class QueryExtensions
    {
        private static object ConvertValue<T>(FaaastQueryDb db, T record, TableMapping mapping, Column column)
        {
            var property = mapping.ColumnToProperty[column];
            var value = property.Read(record);

            var converterType = column.Get(DbMeta.Converter);
            if (converterType != null)
            {
                var converter = (IValueConverter)db.Services.GetRequiredService(converterType);
                value = converter.ToDb(value, property.Type);
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
            sql.AsUpdate(update);

            using var command = await sql.CreateCommandAsync();
            var result = await command.ExecuteNonQueryAsync();
            return result;
        }

        public static async Task<int> InsertAsync<T>(this FaaastQueryDb db, T record)
        {
            var mapping = db.Mapping<T>();
            var sql = new FaaastQuery(db, mapping.Table.Name);

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

            sql.AsInsert(insert, identityColumn != null);

            if (identityColumn == null)
            {
                using var command = await sql.CreateCommandAsync();
                return await command.ExecuteNonQueryAsync();
            }
            else
            {
                var value = sql.FirstOrDefault<object>();
                var converterType = identityColumn.Column.Get(DbMeta.Converter);
                if (converterType != null)
                {
                    var converter = (IValueConverter)db.Services.GetRequiredService(converterType);
                    value = converter.FromDb(value, identityColumn.Property.Type);
                }
                identityColumn.Property.Write(record, value);
            }

            return 1;
        }
    }
}
