using Faaast.DatabaseModel;
using Faaast.Metadata;
using Faaast.Orm.Mapping;

namespace Faaast.Orm
{
    public static class Meta
    {
        public static readonly Metadata<IDatabase, DatabaseMapping> Mapping = new Metadata<IDatabase, DatabaseMapping>(nameof(Mapping));
    }
}
