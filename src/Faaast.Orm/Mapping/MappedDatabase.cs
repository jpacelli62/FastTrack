using System;
using System.Collections.Generic;
using Faaast.DatabaseModel;
using Faaast.Orm.Resolver;

namespace Faaast.Orm.Mapping
{
    internal class MappedDatabase : IDatabase
    {
        private ICollection<TableMapping> _mappings;
        private IDatabase _source;

        public ConnectionSettings Connexion { get => _source.Connexion; set => _source.Connexion = value; }

        public ICollection<Table> Tables { get => _source.Tables; set => _source.Tables = value; }


        public MappedDatabase(IDatabase source)
        {
            this._source = source;
        }


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
