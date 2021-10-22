using Faaast.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Faaast.DatabaseModel
{
    [DebuggerDisplay("{Name}")]
    public class Table : MetaModel<Table>
    {
        public virtual string Name { get; set; }

        public virtual string Schema { get; set; }

        public ICollection<Column> Columns { get; set; } = new List<Column>();
    }
}
