using Faaast.Metadata;
using Faaast.Orm.Mapping;
using Faaast.Orm.Model;

namespace Faaast.Orm
{
    public static class Meta
    {
        public static readonly Metadata<Database, DatabaseMapping> Mapping = new(nameof(Mapping));
    }
}
