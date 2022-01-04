using System.Collections.Generic;
using System.Diagnostics;
using Faaast.Metadata;

namespace Faaast.Orm.Model
{
    [DebuggerDisplay("{Name}")]
    public class Table : MetaModel<Table>
    {
        public virtual string Name { get; set; }

        public virtual string Schema { get; set; }

        public ICollection<Column> Columns { get; set; }

        public Table() => this.Columns = new List<Column>();
    }
}
