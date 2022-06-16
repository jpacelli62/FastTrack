using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Faaast.Metadata;
using Faaast.Orm.Model;

namespace Faaast.Orm.Mapping
{
    public class TableMapping : MetaModel<TableMapping>
    {
        public Table Table { get; set; }

        public string Database { get; set; }

        public IDtoClass ObjectClass { get; set; }

        public ICollection<ColumnMapping> ColumnMappings { get; set; }

        public ReadOnlyDictionary<string, Column> PropertyToColumn { get; private set; }

        public ReadOnlyDictionary<Column, IDtoProperty> ColumnToProperty { get; private set; }

        public void Init()
        {
            var property = new Dictionary<string, Column>(StringComparer.OrdinalIgnoreCase);
            var columns = new Dictionary<Column, IDtoProperty>();
            if (this.ColumnMappings != null)
            {
                foreach (var map in this.ColumnMappings)
                {
                    property.Add(map.Property.Name, map.Column);
                    columns.Add(map.Column, map.Property);
                }
            }

            this.PropertyToColumn = new ReadOnlyDictionary<string, Column>(property);
            this.ColumnToProperty = new ReadOnlyDictionary<Column, IDtoProperty>(columns);
        }
    }
}
