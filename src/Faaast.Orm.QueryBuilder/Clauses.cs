using System;
using System.Reflection;

namespace Faaast.Orm
{
    public class AbstractClause
    {
    }

    public class UnaryClause : AbstractClause
    {

    }
    public class OperationClause : UnaryClause
    {
        public string Function { get; set; }

        public PropertyClause Clause { get; set; }
    }

    public class ConstantClause : UnaryClause
    {
        public object Value { get; set; }
        public ConstantClause(object value) => this.Value = value;
    }

    public class NegateClause : UnaryClause
    {
        public AbstractClause Clause { get; set; }
    }

    public class BinaryColumnClause : AbstractClause
    {
        public AbstractClause Left { get; set; }
        public string Operation { get; set; }
        public AbstractClause Right { get; set; }
    }

    public class PropertyClause : UnaryClause
    {
        public Type ObjectType { get; set; }

        public object References { get; set; }

        public string Property { get; set; }

        public MemberInfo Member { get; set; }
    }
}
