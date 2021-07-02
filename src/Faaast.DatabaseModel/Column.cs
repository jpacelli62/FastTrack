using Faaast.Metadata;

namespace Faaast.DatabaseModel
{
    public class Column : MetaModel<Column>
    {
        public string Name { get; set; }

        public bool Identity { get; set; }

        public bool PrimaryKey { get; set; }

        public Column(string name)
        {
            Name = name;
        }
    }
}
