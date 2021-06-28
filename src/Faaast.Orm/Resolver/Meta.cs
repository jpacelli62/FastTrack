using Faaast.DatabaseModel;
using Faaast.Metadata;

namespace Faaast.Orm.Resolver
{
    public static class Meta
    {
        public static readonly Metadata<DtoClass> PocoObject = new Metadata<DtoClass>(nameof(PocoObject));
        public static readonly Metadata<DtoProperty> PocoProperty = new Metadata<DtoProperty>(nameof(PocoProperty));
        public static readonly Metadata<Column> MappedColumn = new Metadata<Column>(nameof(MappedColumn));
        public static readonly Metadata<Table> MappedTable = new Metadata<Table>(nameof(MappedTable));
    }
}
