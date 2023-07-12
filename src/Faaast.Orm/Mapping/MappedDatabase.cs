using System;
using System.Collections.Generic;
using System.Linq;
using Faaast.Orm.Model;

namespace Faaast.Orm.Mapping
{
    public class DatabaseMapping
    {
        private ICollection<TableMapping> _mappings;

        public Database Source { get; set; }

        public ICollection<TableMapping> Mappings { get => _mappings; set => this.Init(value); }

        public Dictionary<Type, TableMapping> TypeToMapping { get; private set; }

        private void Init(ICollection<TableMapping> value)
        {
            this._mappings = value;
            this.TypeToMapping = value.ToDictionary(x=>x.ObjectClass.Type);
        }
    }
}
