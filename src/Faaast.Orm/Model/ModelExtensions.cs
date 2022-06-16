using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Faaast.Orm.Converters;

namespace Faaast.Orm.Model
{
    public static class ColumnExtensions
    {
        public static Column IsPrimaryKey(this Column column, bool isPromaryKey = true)
        {
            column.PrimaryKey = isPromaryKey;
            return column;
        }

        public static Column IsIdentity(this Column column, bool isIdentity = true)
        {
            column.Identity = isIdentity;
            column.Computed |= isIdentity;
            return column;
        }

        public static Column IsComputed(this Column column, bool isComputed = true)
        {
            column.Computed = isComputed;
            return column;
        }

        public static Column IsNullable(this Column column, bool isNullable = true)
        {
            column.Set(DbMeta.Nullable, isNullable);
            return column;
        }

        public static Column Length(this Column column, int Length)
        {
            column.Set(DbMeta.Length, Length);
            return column;
        }

        public static Column Converter<TConverter>(this Column column) where TConverter: IValueConverter
        {
            column.Set(DbMeta.Converter, typeof(TConverter));
            return column;
        }

        public static Column References(this Column column, string schema, string table, string key)
        {
            column.Set(DbMeta.ForeignKey, !string.IsNullOrEmpty(key));
            column.Set(DbMeta.ReferenceSchema, schema);
            column.Set(DbMeta.ReferenceTable, table);
            column.Set(DbMeta.ReferenceColumn, key);
            return column;
        }

        [Obsolete("Not yet implemented")]
        public static Column References<T>(this Column column, Expression<Func<T, object>> p) =>
            // TODO
            column;

        public static Column[] PrimaryKeyColumns(this Table table)
        {
            var pk = table.Get(DbMeta.PrimaryKeyColumns);
            if (pk == null)
            {
                var columns = new List<Column>();
                foreach (var column in table.Columns)
                {
                    if (column.PrimaryKey)
                    {
                        columns.Add(column);
                    }
                }

                pk = columns.ToArray();
                table.Set(DbMeta.PrimaryKeyColumns, pk);
            }

            return pk;
        }
    }
}
