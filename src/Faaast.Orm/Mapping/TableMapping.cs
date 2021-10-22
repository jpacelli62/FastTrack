using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Faaast.DatabaseModel;
using Faaast.Metadata;

namespace Faaast.Orm
{
    public class TableMapping : MetaModel<TableMapping>
    {
        public Table Table { get; set; }

        public string Database { get; set; }

        public DtoClass ObjectClass { get; set; }

        public ICollection<ColumnMapping> ColumnMappings { get; set; }

        public ReadOnlyDictionary<DtoProperty, Column> PropertyToColumn { get; private set; }

        public ReadOnlyDictionary<Column, DtoProperty> ColumnToProperty { get; private set; }

        internal void Init()
        {
            var property = new Dictionary<DtoProperty, Column>();
            var columns = new Dictionary<Column, DtoProperty>();
            if (this.ColumnMappings != null)
            {
                foreach (var map in ColumnMappings)
                {
                    property.Add(map.Property, map.Column);
                    columns.Add(map.Column, map.Property);
                }
            }

            this.PropertyToColumn = new ReadOnlyDictionary<DtoProperty, Column>(property);
            this.ColumnToProperty = new ReadOnlyDictionary<Column, DtoProperty>(columns);
        }
    }
}
