using System;
using System.Linq.Expressions;


namespace Faaast.Orm
{
    internal static partial class TreeExtensions
	{
		internal static AbstractClause VisitExpression(Expression expression)
		{
			switch (expression.NodeType)
			{
				case ExpressionType.Lambda:
					return VisitExpression(((LambdaExpression)expression).Body);

				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.Equal:
				case ExpressionType.NotEqual:
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				case ExpressionType.Or:
				case ExpressionType.OrElse:

				//case ExpressionType.Subtract:
				//case ExpressionType.Add:
				//case ExpressionType.Divide:
				//case ExpressionType.Multiply:
				//case ExpressionType.Modulo:

					return VisitBinary((BinaryExpression)expression);
					break;

				case ExpressionType.Convert:
				case ExpressionType.Not:
					return VisitUnary((UnaryExpression)expression);
					break;

				//case ExpressionType.New:
				//	return ((NewExpression)expression).Value();

				case ExpressionType.MemberAccess:
					return ((MemberExpression)expression).Value();

				case ExpressionType.Constant:
					return ((ConstantExpression)expression).Value();

				case ExpressionType.Call:
					return VisitLikeExpression((MethodCallExpression)expression);

				//case ExpressionType.Invoke:
				//	VisitExpression(((InvocationExpression)expression).Expression);
				//	break;
			}

			return null;
		}


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
					//return expression.Object.ToString();
				}
			}

			throw new NotImplementedException();
		}

		internal static AbstractClause VisitContainsExpression(MethodCallExpression expression, string textSearch)
		{
			var column = VisitExpression(expression.Object);
			if (expression.Arguments.Count == 0 || expression.Arguments.Count > 1)
			{
				throw new ArgumentException("Contains-expression should contain exactly one argument.", nameof(expression));
			}

			var value = VisitExpression(expression.Arguments[0]);
			string txtOperator;
			switch (textSearch)
            {
                case "contains":
					txtOperator = "contains";
					break;

                case "StartsWith":
					txtOperator = "starts";
					break;

                case "EndsWith":
					txtOperator = "ends";
					break;

                default:
					throw new ArgumentOutOfRangeException($"Invalid TextSearch value '{textSearch}'.", nameof(textSearch));
            }

			return null;
		}


		internal static AbstractClause VisitBinary(BinaryExpression expression)
		{
			BinaryColumnClause result = new BinaryColumnClause();
			var operand = expression.NodeType.GetOperant();
			result.Left = VisitExpression(expression.Left);
			result.Right = VisitExpression(expression.Right);
			result.Operation = operand;
			return result;

		}


		internal static AbstractClause VisitUnary(UnaryExpression expression)
		{
			var result = VisitExpression(expression.Operand);
			switch (expression.NodeType)
			{
				case ExpressionType.Not:
					return new NegateClause { Clause = result };

				case ExpressionType.Convert:
					//if (expression.Method != null)
					//{
					//	condition.Value = Expression.Lambda(expression).Compile().DynamicInvoke();
					//}
					break;
			}

			return null;

		}

	}
}
