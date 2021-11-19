using Faaast.Metadata;

namespace Faaast.DatabaseModel
{
    public static class DbMeta
    {
        public static readonly Metadata<Column, bool?> ForeignKey = new(nameof(ForeignKey));
        public static readonly Metadata<Column, string> ReferenceSchema = new(nameof(ReferenceSchema));
        public static readonly Metadata<Column, string> ReferenceTable = new(nameof(ReferenceTable));
        public static readonly Metadata<Column, string> ReferenceColumn = new(nameof(ReferenceColumn));
        public static readonly Metadata<Column, bool?> Nullable = new(nameof(Nullable));
        public static readonly Metadata<Column, int?> Length = new(nameof(Length));
        public static readonly Metadata<Table, Column[]> PrimaryKeyColumns = new(nameof(PrimaryKeyColumns));
    }
}
