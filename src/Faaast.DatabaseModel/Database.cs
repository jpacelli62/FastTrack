using Faaast.Metadata;
using System;
using System.Collections.Generic;

namespace Faaast.DatabaseModel
{
    public class Database : MetaModel<IDatabase>, IDatabase
    {
        public ConnectionSettings Connexion { get; set; }

        public ICollection<Table> Tables { get; set; }

        public Database(ConnectionSettings connexion)
        {
            this.Connexion = connexion;
            this.Tables = new List<Table>();
        }
    }
}
