using System;
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
            return new ConstantClause(getter());
        }

        internal static ConstantClause Value(this ConstantExpression expression) => new(expression.Value);

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
                var expType = expression.Expression?.Type;
                if (expType == typeof(string))
                {
                    if (string.Equals(expression.Member.Name, "Length", StringComparison.OrdinalIgnoreCase))
                    {
                        return new OperationClause
                        {
                            Clause = Value(expression.Expression as MemberExpression) as PropertyClause,
                            Function = "LEN([{0}])"
                        };
                    }
                }

                var member = Expression.Convert(expression, typeof(object));
                var lambda = Expression.Lambda<Func<object>>(member);
                var getter = lambda.Compile();
                return new ConstantClause(getter());
            }

            throw new NotImplementedException();
        }

        internal static string GetOperant(this ExpressionType expressionType) => expressionType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            ExpressionType.Add => "+",
            ExpressionType.Subtract => "-",
            ExpressionType.Multiply => "*",
            ExpressionType.Divide => "/",
            ExpressionType.Modulo => "MOD",
            ExpressionType.Coalesce => "COALESCE",
            _ => expressionType.ToString()
        };
    }
}
