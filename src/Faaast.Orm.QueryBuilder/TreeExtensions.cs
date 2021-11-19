using System;
using System.Linq.Expressions;

namespace Faaast.Orm
{
    internal static partial class TreeExtensions
    {
        internal static AbstractClause VisitExpression(Expression expression) => expression.NodeType switch
        {
            ExpressionType.Lambda => VisitExpression(((LambdaExpression)expression).Body),
            ExpressionType.MemberAccess => ((MemberExpression)expression).Value(),
            ExpressionType.Constant => ((ConstantExpression)expression).Value(),
            ExpressionType.Call => VisitLikeExpression((MethodCallExpression)expression),

            ExpressionType.LessThan or
            ExpressionType.LessThanOrEqual or
            ExpressionType.GreaterThan or
            ExpressionType.GreaterThanOrEqual or
            ExpressionType.Equal or
            ExpressionType.NotEqual or
            ExpressionType.And or
            ExpressionType.AndAlso or
            ExpressionType.Or or
            ExpressionType.OrElse => VisitBinary((BinaryExpression)expression),//case ExpressionType.Subtract:

            ExpressionType.Convert or
            ExpressionType.Not => VisitUnary((UnaryExpression)expression),

            //case ExpressionType.New:
            //	return ((NewExpression)expression).Value();
            _ => null,
        };

        internal static AbstractClause VisitLikeExpression(MethodCallExpression expression)
        {
            var method = expression.Method.Name.ToLower();
            if (expression.Object != null)
            {
                if (expression.Object.Type == typeof(string))
                {
                    return VisitContainsExpression(expression, method);
                }
                else if (method == "tostring")
                {
                    throw new NotImplementedException();
                }
            }

            throw new NotImplementedException();
        }

        internal static AbstractClause VisitContainsExpression(MethodCallExpression expression, string textSearch)
        {
            var column = VisitExpression(expression.Object);
            if (expression.Arguments.Count is 0 or > 1)
            {
                throw new ArgumentException("Contains-expression should contain exactly one argument.", nameof(expression));
            }

            var value = (ConstantClause)VisitExpression(expression.Arguments[0]);
            return textSearch switch
            {
                "contains" => new BinaryColumnClause()
                {
                    Left = column,
                    Operation = "LIKE",
                    Right = new ConstantClause(string.Concat("%", value.Value, "%"))
                },
                "startswith" => new BinaryColumnClause()
                {
                    Left = column,
                    Operation = "LIKE",
                    Right = new ConstantClause(string.Concat(value.Value, "%"))
                },
                "endswith" => new BinaryColumnClause()
                {
                    Left = column,
                    Operation = "LIKE",
                    Right = new ConstantClause(string.Concat("%", value.Value))
                },
                _ => throw new ArgumentOutOfRangeException(nameof(textSearch), $"Invalid TextSearch value '{textSearch}'."),
            };
            throw new NotImplementedException();
        }

        internal static AbstractClause VisitBinary(BinaryExpression expression)
        {
            var result = new BinaryColumnClause();
            var operand = expression.NodeType.GetOperant();
            result.Left = VisitExpression(expression.Left);
            result.Right = VisitExpression(expression.Right);
            result.Operation = operand;
            return result;

        }

        internal static AbstractClause VisitUnary(UnaryExpression expression)
        {
            var result = VisitExpression(expression.Operand);
            return expression.NodeType switch
            {
                ExpressionType.Not => new NegateClause { Clause = result },
                ExpressionType.Convert => result,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
