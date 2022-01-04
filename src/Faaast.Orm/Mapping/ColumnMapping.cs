using System.Reflection;
using Faaast.Metadata;
using Faaast.Orm.Model;

namespace Faaast.Orm.Mapping
{
    public class ColumnMapping : MetaModel<ColumnMapping>
    {
        public IDtoProperty Property { get; set; }

        public MemberInfo Member { get; set; }

        public Column Column { get; set; }
    }
}
