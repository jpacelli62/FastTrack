using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace Faaast.Orm
{
    internal static class ValueGetters
    {
        internal static object Value(this NewExpression expression)
        {
            var member = Expression.Convert(expression, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            return new ConstantClause { Value = getter()};
        }

        internal static ConstantClause Value(this ConstantExpression expression) => new ConstantClause { Value = expression.Value };

        internal static AbstractClause Value(this MemberExpression expression)
        {
            if (expression.Expression?.NodeType == ExpressionType.Parameter)
            {
                return new PropertyClause
                {
                    Member = expression.Member,
                    ObjectType = expression.Expression.Type,
                    Property = expression.Member.Name,
                    References = ((ParameterExpression)expression.Expression).Name
                };
            } 
            else if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var member = Expression.Convert(expression, typeof(object));
                var lambda = Expression.Lambda<Func<object>>(member);
                var getter = lambda.Compile();
                return new ConstantClause { Value = getter() };
            }

            throw new NotImplementedException();
        }

        internal static string GetOperant(this ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Equal: return "=";
                case ExpressionType.NotEqual: return "<>";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.AndAlso: return "AND";
                case ExpressionType.OrElse: return "OR";
                case ExpressionType.Add: return "+";
                case ExpressionType.Subtract: return "-";
                case ExpressionType.Multiply: return "*";
                case ExpressionType.Divide: return "/";
                case ExpressionType.Modulo: return "MOD";
                case ExpressionType.Coalesce: return "COALESCE";
                default:
                    return expressionType.ToString();
            }
        }
    }
}
