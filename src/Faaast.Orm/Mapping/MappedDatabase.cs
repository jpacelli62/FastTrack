using Faaast.DatabaseModel;
using System;
using System.Collections.Generic;

namespace Faaast.Orm.Mapping
{
    public class DatabaseMapping
    {
        private ICollection<TableMapping> _mappings;

        public IDatabase Source { get; set; }

        public ICollection<TableMapping> Mappings { get => _mappings; set => Init(value); }

        public Dictionary<Type, Table> TypeToTable { get; private set; }
        
        public Dictionary<Type, TableMapping> TypeToMapping { get; private set; }

        private void Init(ICollection<TableMapping> value)
        {
            this._mappings = value;
            TypeToTable = new Dictionary<Type, Table>();
            TypeToMapping = new Dictionary<Type, TableMapping>();
            foreach (var map in value)
            {
                TypeToTable.Add(map.ObjectClass.Type, map.Table);
                TypeToMapping.Add(map.ObjectClass.Type, map);
            }
        }
    }
}
