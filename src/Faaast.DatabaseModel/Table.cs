using Faaast.Metadata;
using System;
using System.Collections.Generic;

namespace Faaast.DatabaseModel
{
    public class Table : MetaModel
    {
        public virtual string Name { get; set; }
        public virtual string Schema { get; set; }
        public IDictionary<string, Column> Columns { get; set; } = new Dictionary<string, Column>(StringComparer.OrdinalIgnoreCase);
    }
}
