using Faaast.Metadata;
using System;
using System.Collections.Generic;

namespace Faaast.DatabaseModel
{
    public class Database : MetaModel, IDatabase
    {
        public ConnectionSettings Connexion { get; set; }

        public IDictionary<string, Table> Tables { get; set; }

        public Database(ConnectionSettings connexion)
        {
            this.Connexion = connexion;
            this.Tables = new Dictionary<string, Table>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
