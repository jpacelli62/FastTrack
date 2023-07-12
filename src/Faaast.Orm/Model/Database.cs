using System.Collections.Generic;
using System.Diagnostics;
using Faaast.Metadata;
using Faaast.Orm.Mapping;

namespace Faaast.Orm.Model
{
    [DebuggerDisplay("{Connexion.Name}")]
    public class Database : MetaModel<Database>
    {
        public ConnectionSettings Connexion { get; set; }

        public ICollection<Table> Tables { get; set; }

        public DatabaseMapping Mapping { get; set; }

        public Database(ConnectionSettings connexion)
        {
            this.Connexion = connexion;
            this.Tables = new List<Table>();
        }
    }
}
