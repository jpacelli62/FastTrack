using System;
using System.Linq.Expressions;
using Faaast.Orm.Mapping;

namespace Faaast.Orm
{
    public class TableAlias<T>
    {
        private readonly string[] _allColumns;

        public TableMapping Mapping { get; private set; }

        public string Alias { get; private set; }

        public string this[Expression<Func<T, object>> property]
        {
            get {
                var body = property.Body;
                switch (body.NodeType)
                {
                    case ExpressionType.Convert:
                        return ((UnaryExpression)body).Operand is MemberExpression member ? this[member.Member.Name] : throw new ArgumentException("Cannot read property value");
                    case ExpressionType.MemberAccess:
                        return this[((MemberExpression)body).Member.Name];
                }

                throw new NotImplementedException();
            }
        }

        public string this[string property] => this.Mapping.PropertyToColumn.TryGetValue(property, out var column) ?
                    this.FormatPrefix(column.Name) :
                    throw new ArgumentException($"Property \"{this.Mapping.ObjectClass.Name}.{property}\" is not mapped to a database column");

        private string FormatPrefix(string value) => string.IsNullOrWhiteSpace(this.Alias) ? value : string.Concat(this.Alias, '.', value);

        private string FormatSuffix(string value) => string.IsNullOrWhiteSpace(this.Alias) ? value : string.Concat(value, " as ", this.Alias);

        public TableAlias(TableMapping mapping, string alias = null)
        {
            this.Mapping = mapping;
            this.Alias = alias;
            _allColumns = new string[mapping.Table.Columns.Count];
            var i = 0;
            foreach (var column in mapping.Table.Columns)
            {
                _allColumns[i++] = this.FormatPrefix(column.Name);
            }
        }

        public string[] AllColumns => _allColumns;

        public static implicit operator string(TableAlias<T> alias) => alias.FormatSuffix(alias.Mapping.Table.Name);
    }
}
