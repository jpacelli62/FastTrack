using System.Collections.Generic;

namespace Faaast.DatabaseModel
{
    public interface IDatabase
    {
        ConnectionSettings Connexion { get; set; }

        IDictionary<string, Table> Tables { get; set; }
    }
}
