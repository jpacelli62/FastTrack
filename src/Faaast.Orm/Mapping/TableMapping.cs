﻿using System;
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

        public ReadOnlyDictionary<string, Column> PropertyToColumn { get; private set; }

        public ReadOnlyDictionary<Column, DtoProperty> ColumnToProperty { get; private set; }

        internal void Init()
        {
            var property = new Dictionary<string, Column>();
            var columns = new Dictionary<Column, DtoProperty>();
            if (this.ColumnMappings != null)
            {
                foreach (var map in ColumnMappings)
                {
                    property.Add(map.Property.Name, map.Column);
                    columns.Add(map.Column, map.Property);
                }
            }

            this.PropertyToColumn = new ReadOnlyDictionary<string, Column>(property);
            this.ColumnToProperty = new ReadOnlyDictionary<Column, DtoProperty>(columns);
        }
    }
}
