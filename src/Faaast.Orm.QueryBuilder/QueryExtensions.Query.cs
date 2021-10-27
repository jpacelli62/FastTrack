using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SqlKata;

namespace Faaast.Orm
{
    public static partial class QueryExtensions
    {
        internal static void ConvertExpression<TClass>(FaaastQueryDb db, Faaast.Orm.AbstractClause clause, Query query)
        {
            if (clause is BinaryColumnClause binary)
            {
                if (binary.Left is UnaryClause leftUnary && binary.Right is UnaryClause rightUnary)
                {
                    ConvertWhere<TClass>(db, binary, leftUnary, rightUnary, query);
                }
                else
                {
                    query.Where(q =>
                    {
                        ConvertExpression<TClass>(db, binary.Left, q);
                        if (string.Compare("or", binary.Operation, true) == 0)
                            q.Or();
                        ConvertExpression<TClass>(db, binary.Right, q);

                        return q;
                    });
                }
            }
            else if (clause is PropertyClause property)// Implicit true
            {
                query.Where(property.Property, "=", 1);
            }
            else if (clause is NegateClause negate)
            {
                query.Not();
                ConvertExpression<TClass>(db, negate.Clause, query);
            }
        }

        internal static Query ConvertWhere<TClass>(FaaastQueryDb db, BinaryColumnClause binary, UnaryClause leftUnary, UnaryClause rightUnary, Query query)
        {
            UnaryClause[] values = leftUnary is PropertyClause ? new[] { leftUnary, rightUnary } : new[] { rightUnary, leftUnary };
            if (values[0] is PropertyClause)
            {
                string property = ((PropertyClause)values[0]).Property;
                return query.Where(property, binary.Operation, ConvertValue<TClass>(db, values[1]));
            }

            throw new NotImplementedException();
        }
        internal static object ConvertValue<TClass>(FaaastQueryDb db, UnaryClause clause)
        {
            if (clause is ConstantClause constant)
                return constant.Value;
            else if (clause is PropertyClause property)
            {
                return db.Mapping<TClass>().PropertyToColumn[property.Property];
            }

            throw new NotImplementedException();
        }

        public static FaaastQuery<TClass> Where<TClass>(this FaaastQuery<TClass> query, Expression<Func<TClass, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertExpression<TClass>(query.Db, result, query);
            return query;
        }

        public static FaaastQuery<TClass> From<TClass>(this FaaastQuery query, string alias = null)
        {
            var mapping = query.Db.Mapping<TClass>();
            string tableName = mapping.Table.Name;
            if (string.IsNullOrWhiteSpace(alias))
            {
                query.From(tableName);
            }
            else
            {
                query.From(string.Concat(tableName, " AS ", alias));
            }

            return query.Clone<TClass>();
        }

        public static Task<TClass> FirstOrDefaultAsync<TClass>(this FaaastQuery<TClass> query,
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var compiledQuery = query.Compile();
            var command = query.Db.Query(
                compiledQuery.Sql,
                compiledQuery.Parameters,
                connection,
                transaction,
                commandTimeout,
                CommandType.Text,
                cancellationToken
                );

            return command.FirstOrDefaultAsync<TClass>();
        }

        public static async Task<ICollection<TClass>> ToListAsync<TClass>(this FaaastQuery<TClass> query,
            DbConnection connection = null,
            DbTransaction transaction = null,
            int? commandTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var compiledQuery = query.Compile();
            var command = query.Db.Query(
                compiledQuery.Sql,
                compiledQuery.Parameters,
                connection,
                transaction,
                commandTimeout,
                CommandType.Text,
                cancellationToken
                );

            List<TClass> result = new List<TClass>();
            await foreach (var row in command.FetchAsync<TClass>())
            {
                result.Add(row);
            }

            return result;
        }

        public static FaaastQuery<TClass> Top<TClass>(this FaaastQuery<TClass> query, int nb)
        {
            query.Limit(nb);
            return query;
        }

        public static FaaastQuery<TClass> OrderBy<TClass, TProperty>(this FaaastQuery<TClass> query, Expression<Func<TClass, TProperty>> exp, string order = "ASC")
        {
            var clause = TreeExtensions.VisitExpression(exp);
            var mapping = query.Db.Mapping<TClass>();

            if (clause is PropertyClause prop)
            {
                query.OrderBy(mapping.PropertyToColumn[prop.Property].Name);
                return query;
            }
            else if (clause is OperationClause op)
            {
                query.OrderByRaw(string.Concat(string.Format(op.Function, mapping.PropertyToColumn[op.Clause.Property].Name), " ASC"));
                return query;
            }
            else
                throw new ArgumentException(nameof(exp));
        }



        public static FaaastQuery<TClass> OrderByDescending<TClass>(this FaaastQuery<TClass> query, Expression<Func<TClass, object>> exp)
        {
            var clause = TreeExtensions.VisitExpression(exp);
            var mapping = query.Db.Mapping<TClass>();

            if (clause is PropertyClause prop)
            {
                query.OrderByDesc(mapping.PropertyToColumn[prop.Property].Name);
                return query;
            }
            else if (clause is OperationClause op)
            {
                query.OrderByRaw(string.Concat(string.Format(op.Function, mapping.PropertyToColumn[op.Clause.Property].Name)), " DESC");
                return query;
            }
            else
                throw new ArgumentException(nameof(exp));
        }
    }
}
