using System.Reflection;
using Faaast.DatabaseModel;
using Faaast.Metadata;

namespace Faaast.Orm.Mapping
{
    public class ColumnMapping : MetaModel<ColumnMapping>
    {
        public DtoProperty Property { get; set; }

        public MemberInfo Member { get; set; }

        public Column Column { get; set; }
    }
}
