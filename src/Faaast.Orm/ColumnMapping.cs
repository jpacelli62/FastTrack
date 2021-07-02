using System.Reflection;
using Faaast.DatabaseModel;
using Faaast.Metadata;

namespace Faaast.Orm
{
    public class ColumnMapping: MetaModel<ColumnMapping>
    {
        public DtoProperty Property { get; set; }

        public MemberInfo Member { get; set; }

        public Column Column { get; set; }

        public Column ToColumn(string name)
        {
            this.Column = new Column(name);
            return this.Column;
        }
    }
}
