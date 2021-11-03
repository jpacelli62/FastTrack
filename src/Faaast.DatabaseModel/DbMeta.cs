using Faaast.Metadata;

namespace Faaast.DatabaseModel
{
    public static class DbMeta
    {
        public static readonly Metadata<Column, bool?> ForeignKey = new Metadata<Column, bool?>(nameof(ForeignKey));
        public static readonly Metadata<Column, string> ReferenceSchema = new Metadata<Column, string>(nameof(ReferenceSchema));
        public static readonly Metadata<Column, string> ReferenceTable = new Metadata<Column, string>(nameof(ReferenceTable));
        public static readonly Metadata<Column, string> ReferenceColumn = new Metadata<Column, string>(nameof(ReferenceColumn));
        public static readonly Metadata<Column, bool?> Nullable = new Metadata<Column, bool?>(nameof(Nullable));
        public static readonly Metadata<Column, int?> Length = new Metadata<Column, int?>(nameof(Length));
        public static readonly Metadata<Table, Column[]> PrimaryKeyColumns = new Metadata<Table, Column[]>(nameof(PrimaryKeyColumns));
    }
}
