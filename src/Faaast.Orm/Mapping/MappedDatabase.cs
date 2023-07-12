using System;
using System.Collections.Generic;
using Faaast.Orm.Model;

namespace Faaast.Orm.Mapping
{
    public class DatabaseMapping
    {
        private ICollection<TableMapping> _mappings;

        public Database Source { get; set; }

        public ICollection<TableMapping> Mappings { get => _mappings; set => this.Init(value); }

        public Dictionary<Type, Table> TypeToTable { get; private set; }

        public Dictionary<Type, TableMapping> TypeToMapping { get; private set; }

        private void Init(ICollection<TableMapping> value)
        {
            this._mappings = value;
            this.TypeToTable = new Dictionary<Type, Table>();
            this.TypeToMapping = new Dictionary<Type, TableMapping>();
            foreach (var map in value)
            {
                this.TypeToTable.Add(map.ObjectClass.Type, map.Table);
                this.TypeToMapping.Add(map.ObjectClass.Type, map);
            }
        }
    }
}
