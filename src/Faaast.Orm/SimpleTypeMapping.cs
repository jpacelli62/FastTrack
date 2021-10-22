using Faaast.DatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Faaast.Orm
{
    public class SimpleTypeMapping
    {
        public Type Type { get; set; }

        public TableMapping Table { get; set; } = new TableMapping();

        public SimpleTypeMapping(Type type)
        {
            this.Type = type;
        }

        public void ToTable(string name, string schema = null)
        {
            Table = new TableMapping
            {
                Table = new Table
                {
                    Name = name,
                    Schema = schema
                }
            };
        }

        public void ToTable(Table table)
        {
            Table = new TableMapping
            {
                Table = table
            };
        }

        public void ToDatabase(string name)
        {
            this.Table.Database = name;
        }
    }

    public class SimpleTypeMapping<TClass> : SimpleTypeMapping where TClass : class
    {
        public SimpleTypeMapping() : base(typeof(TClass))
        {
        }


        public Column Map<TProperty>(Expression<Func<TClass, TProperty>> member, string columnName)
        {
            return Map<TProperty>(member, new Column(columnName));
        }

        public Column Map<TProperty>(Expression<Func<TClass, TProperty>> member, Column column)
        {
            var exp = member.Body as MemberExpression;
            if(exp == null)
            {
                throw new ArgumentException(nameof(member));
            }

            ColumnMapping mapping = new ColumnMapping();
            mapping.Member = exp.Member;
            mapping.Column = column;
            var mappings = Table.ColumnMappings ?? new List<ColumnMapping>();
            mappings.Add(mapping);
            Table.ColumnMappings = mappings;
            Table.Table.Columns.Add(column);
            return mapping.Column;
        }
    }
}
