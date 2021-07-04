using System.Collections.Generic;
using System.Collections.ObjectModel;
using Faaast.DatabaseModel;
using Faaast.Metadata;

namespace Faaast.Orm
{
    public class TableMapping : MetaModel<TableMapping>
    {
        private ICollection<ColumnMapping> _mappings;

        public Table Table { get; set; }

        public string Database { get; set; }

        public DtoClass ObjectClass { get; set; }

        public ICollection<ColumnMapping> ColumnMappings { get => _mappings; set => Init(value); }

        public ReadOnlyDictionary<DtoProperty, Column> PropertyToColumn { get; private set; }

        public ReadOnlyDictionary<Column, DtoProperty> ColumnToProperty { get; private set; }

        private void Init(ICollection<ColumnMapping> value)
        {
            this._mappings = value;
            var property = new Dictionary<DtoProperty, Column>();
            var columns = new Dictionary<Column, DtoProperty>();

            foreach (var map in value)
            {
                property.Add(map.Property, map.Column);
                columns.Add(map.Column, map.Property);
            }

            this.PropertyToColumn = new ReadOnlyDictionary<DtoProperty, Column>(property);
            this.ColumnToProperty = new ReadOnlyDictionary<Column, DtoProperty>(columns);
        }
    }
}
