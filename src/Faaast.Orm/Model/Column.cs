using System.Diagnostics;
using Faaast.Metadata;

namespace Faaast.Orm.Model
{
    [DebuggerDisplay("{Name}")]
    public class Column : MetaModel<Column>
    {
        public string Name { get; set; }

        public bool Identity { get; set; }

        public bool Computed { get; set; }

        public bool PrimaryKey { get; set; }

        public Column(string name) => this.Name = name;
    }
}
