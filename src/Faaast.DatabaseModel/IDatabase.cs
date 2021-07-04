using Faaast.Metadata;
using System.Collections.Generic;

namespace Faaast.DatabaseModel
{
    public interface IDatabase : IMetaModel<IDatabase>
    {
        ConnectionSettings Connexion { get; set; }

        ICollection<Table> Tables { get; set; }
    }
}
