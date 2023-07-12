using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Faaast.Metadata;
using Faaast.Orm.Converters;

namespace Faaast.Orm.Model
{
    [DebuggerDisplay("{Name}")]
    public class Column : MetaModel<Column>
    {
        public string Name { get; set; }

        public bool Identity { get; set; }

        public bool Computed { get; set; }

        public bool PrimaryKey { get; set; }

        public Column(string name) => this.Name = name;

        public bool? ForeignKey { get; set; }

        public string ReferenceSchema { get; set; }

        public string ReferenceTable { get; set; }

        public string ReferenceColumn { get; set; }

        public bool? Nullable { get; set; }

        public int? DataLength { get; set; }

        public Type ConverterType { get; set; }

        public IValueConverter ConverterInstance { get; set; }

        public Column[] PrimaryKeyColumns { get; set; }

        public Column IsPrimaryKey( bool isPrimaryKey = true)
        {
            this.PrimaryKey = isPrimaryKey;
            return this;
        }

        public Column IsIdentity(bool isIdentity = true)
        {
            this.Identity = isIdentity;
            this.Computed |= isIdentity;
            return this;
        }

        public Column IsComputed(bool isComputed = true)
        {
            this.Computed = isComputed;
            return this;
        }

        public Column IsNullable(bool isNullable = true)
        {
            this.Nullable = isNullable;
            return this;
        }

        public Column Length(int Length)
        {
            this.DataLength = Length;
            return this;
        }

        public Column Converter<TConverter>() where TConverter : IValueConverter
        {
            this.ConverterType = typeof(TConverter);
            return this;
        }

        public Column References(string schema, string table, string key)
        {
            this.ForeignKey = !string.IsNullOrEmpty(key);
            this.ReferenceSchema = schema;
            this.ReferenceTable = table;
            this.ReferenceColumn = key;
            return this;
        }

        [Obsolete("Not yet implemented")]
        public Column References<T>(Expression<Func<T, object>> p) =>
            // TODO
             this;
    }
}
