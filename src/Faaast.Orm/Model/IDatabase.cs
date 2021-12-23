using System.Collections.Generic;
using Faaast.Metadata;

namespace Faaast.Orm.Model
{
    public interface IDatabase : IMetaModel<IDatabase>
    {
        ConnectionSettings Connexion { get; set; }

        ICollection<Table> Tables { get; set; }
    }
}
