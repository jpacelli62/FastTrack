using Faaast.Metadata;

namespace Faaast.DatabaseModel
{
    public static class DbMeta
    {
        public static readonly Metadata<bool> ForeignKey = new Metadata<bool>(nameof(ForeignKey));
        public static readonly Metadata<string> ReferenceSchema = new Metadata<string>(nameof(ReferenceSchema));
        public static readonly Metadata<string> ReferenceTable = new Metadata<string>(nameof(ReferenceTable));
        public static readonly Metadata<string> ReferenceColumn = new Metadata<string>(nameof(ReferenceColumn));
        public static readonly Metadata<bool> Nullable = new Metadata<bool>(nameof(Nullable));
        public static readonly Metadata<int> Length = new Metadata<int>(nameof(Length));
        public static readonly Metadata<Column[]> PrimaryKeyColumns = new Metadata<Column[]>(nameof(PrimaryKeyColumns));
    }
}
