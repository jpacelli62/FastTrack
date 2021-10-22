using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Faaast.Orm
{
    public class AbstractClause
    {
    }

    public class ConstantClause : AbstractClause
    {
        public object Value { get; set; }
    }

    public class NegateClause : AbstractClause
    {
        public AbstractClause Clause { get; set; }
    }

    public class BinaryColumnClause : AbstractClause
    {
        public AbstractClause Left { get; set; }
        public string Operation { get; set; }
        public AbstractClause Right { get; set; }
    }

    public class PropertyClause : AbstractClause
    {
        public Type ObjectType { get; set; }

        public object References { get; set; }

        public string Property { get; set; }

        public MemberInfo Member { get; set; }
    }
}
