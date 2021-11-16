using Faaast.Orm.Mapping;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Faaast.Orm
{
    public class FaaastQuery : FaaastQueryBase<FaaastQuery>
    {
        public FaaastQuery(FaaastQueryDb db) : base(db)
        {
        }

        public override FaaastQuery NewQuery()
        {
            return new FaaastQuery(base.Db);
        }

        public CompiledQuery Compile()
        {

            return base.Db.Compile(this);
        }

        public FaaastQuery<TModel> Clone<TModel>()
        {
            var clone = new FaaastQuery<TModel>(this.Db);
            clone.Query = Query;
            clone.LastAlias = Query.QueryAlias;
            return clone;
        }

        public FaaastQuery<TClass> From<TClass>(string alias = null)
        {
            var clone = Clone<TClass>();
            if (string.IsNullOrWhiteSpace(alias))
            {
                clone.Query.From(clone.Mapping.Table.Name);
            }
            else
            {
                clone.LastAlias = alias;
                clone.Query.As(alias);
                clone.Query.FromRaw($"[{clone.Mapping.Table.Name}] AS [{alias}]");
            }

            return clone;
        }
    }

    public class FaaastQuery<TModel> : FaaastQuery
    {
        internal TableMapping Mapping { get; private set; }

        internal string LastAlias { get; set; }

        public FaaastQuery(FaaastQueryDb db) : base(db)
        {
            Mapping = Db.Mapping<TModel>();
        }

        public FaaastQuery<TModel> OrderBy<TProperty>(Expression<Func<TModel, TProperty>> exp, string order = "ASC")
        {
            var clause = TreeExtensions.VisitExpression(exp);

            if (clause is PropertyClause prop)
            {
                Query.OrderBy(Mapping.PropertyToColumn[prop.Property].Name);
            }
            else if (clause is OperationClause op)
            {
                Query.OrderByRaw(string.Concat(string.Format(op.Function, Mapping.PropertyToColumn[op.Clause.Property].Name), " ASC"));
            }
            else
                throw new ArgumentException(nameof(exp));

            return this;
        }

        public FaaastQuery<TModel> OrderByDescending<TProperty>(Expression<Func<TModel, TProperty>> exp)
        {
            var clause = TreeExtensions.VisitExpression(exp);
            if (clause is PropertyClause prop)
            {
                Query.OrderByDesc(Mapping.PropertyToColumn[prop.Property].Name);
            }
            else if (clause is OperationClause op)
            {
                Query.OrderByRaw(string.Concat(string.Format(op.Function, Mapping.PropertyToColumn[op.Clause.Property].Name)), " DESC");
            }
            else
                throw new ArgumentException(nameof(exp));
            return this;
        }

        public FaaastQuery<TModel> Top(int nb)
        {
            Query.Limit(nb);
            return this;
        }

        public FaaastQuery<TModel> Where(Dictionary<string, object> keyValuePairs)
        {
            Query.Where(keyValuePairs);
            return this;
        }

        internal FaaastQuery<TModel> AsInsert(Dictionary<string, object> insert, bool returnId)
        {
            Query.AsInsert(insert, returnId);
            return this;
        }

        public FaaastQuery<TModel> AsUpdate(Dictionary<string, object> update)
        {
            Query.AsUpdate(update);
            return this;
        }

        public FaaastQuery<TModel> AsDelete()
        {
            Query.AsDelete();
            return this;
        }

        public FaaastQuery<TModel> SelectFields()
        {
            if (!string.IsNullOrWhiteSpace(LastAlias))
            {
                foreach (var map in Mapping.ColumnMappings)
                {
                    Query.Select(string.Concat(LastAlias, ".", map.Column.Name));
                }
            }
            else
            {
                foreach (var map in Mapping.ColumnMappings)
                {
                    Query.Select(map.Column.Name);
                }
            }

            return this;
        }

        public FaaastQuery<TModel> Select<V>(string alias, params Expression<Func<TModel, V>>[] exps)
        {
            foreach (var exp in exps)
            {
                var result = TreeExtensions.VisitExpression(exp);
                if (result is PropertyClause property)
                {
                    Query.Select(ConvertValue(property).ToString());
                }
            }

            return this;
        }

        public FaaastQuery<T> Select<T>(string alias)
        {
            var mapping = Db.Mapping<T>();
            if (!string.IsNullOrWhiteSpace(alias))
            {
                foreach (var map in mapping.ColumnMappings)
                {
                    Query.Select(string.Concat(alias, ".", map.Column.Name));
                }
            }
            else
            {
                foreach (var map in mapping.ColumnMappings)
                {
                    Query.Select(map.Column.Name);
                }
            }

            return this.Clone<T>();
        }

        public FaaastQuery<TModel> Where(Expression<Func<TModel, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }

        public FaaastQuery<TModel> Where<A>(Expression<Func<A, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B>(Expression<Func<A, B, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C>(Expression<Func<A, B, C, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D>(Expression<Func<A, B, C, D, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E>(Expression<Func<A, B, C, D, E, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F>(Expression<Func<A, B, C, D, E, F, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G>(Expression<Func<A, B, C, D, E, F, G, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H>(Expression<Func<A, B, C, D, E, F, G, H, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I>(Expression<Func<A, B, C, D, E, F, G, H, I, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I, J>(Expression<Func<A, B, C, D, E, F, G, H, I, J, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }



        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I, J, K>(Expression<Func<A, B, C, D, E, F, G, H, I, J, K, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I, J, K, L>(Expression<Func<A, B, C, D, E, F, G, H, I, J, K, L, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I, J, K, L, M>(Expression<Func<A, B, C, D, E, F, G, H, I, J, K, L, M, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I, J, K, L, M, N>(Expression<Func<A, B, C, D, E, F, G, H, I, J, K, L, M, N, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O>(Expression<Func<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            ConvertWhere(result);
            return this;
        }


        internal void ConvertWhere(AbstractClause clause, Query q = null)
        {
            if (clause is BinaryColumnClause binary)
            {
                if (binary.Left is UnaryClause leftUnary && binary.Right is UnaryClause rightUnary)
                {
                    (q ?? Query).Where(ConvertValue(leftUnary).ToString(), binary.Operation, ConvertValue(rightUnary));
                    return;
                }
                else
                {
                    (q ?? Query).Where(q =>
                    {
                        ConvertWhere(binary.Left, q);
                        if (string.Compare("or", binary.Operation, true) == 0)
                            q.Or();
                        ConvertWhere(binary.Right, q);
                        return q;
                    });
                    return;
                }
            }
            else if (clause is PropertyClause property)// Implicit true
            {
                (q ?? Query).Where(ConvertValue(property).ToString(), "=", 1);
                return;
            }
            else if (clause is NegateClause negate)
            {
                (q ?? Query).Not();
                ConvertWhere(negate.Clause, q);
                return;
            }

            throw new NotImplementedException();
        }

        internal void ConvertJoin(AbstractClause clause, Join join)
        {
            if (clause is BinaryColumnClause binary)
            {
                if (binary.Left is UnaryClause leftUnary && binary.Right is UnaryClause rightUnary)
                {
                    join.On(ConvertValue(leftUnary).ToString(), ConvertValue(rightUnary).ToString(), binary.Operation);
                    return;
                }
                else
                {
                    ConvertJoin(binary.Left, join);
                    if (string.Compare("or", binary.Operation, true) == 0)
                        join.Or();
                    ConvertJoin(binary.Right, join);
                    return;
                }
            }
            else if (clause is PropertyClause property)// Implicit true
            {
                join.AddComponent("where", new BasicCondition
                {
                    Column = ConvertValue(property).ToString(),
                    Operator = "=",
                    Value = 1,
                    IsOr = false,
                    IsNot = false,
                });
                //join.On(ConvertValue(property).ToString(), "=", 1);
                return;
            }
            else if (clause is NegateClause negate)
            {
                Query.Not();
                ConvertJoin(negate.Clause, join);
                return;
            }

            throw new NotImplementedException();
        }
        private FaaastQuery<TModelB> Join<TModelA, TModelB>(string alias, Expression<Func<TModelA, TModelB, bool>> join, string type)
        {
            FaaastQuery<TModelB> clone = Clone<TModelB>();
            clone.LastAlias = alias;
            AbstractClause clause = TreeExtensions.VisitExpression(join);
            clone.Query.Join($"{clone.Mapping.Table.Name} AS {alias}", q =>
            {
                ConvertJoin(clause, q);
                return q;
            }, type);

            return clone;
        }

        public FaaastQuery<TModelB> InnerJoin<TModelB>(string alias, Expression<Func<TModel, TModelB, bool>> join)
        {
            return Join(alias, join, "inner join");
        }

        public FaaastQuery<TModelB> LeftJoin<TModelB>(string alias, Expression<Func<TModel, TModelB, bool>> join)
        {
            return Join(alias, join, "left join");
        }

        public FaaastQuery<TModelB> RightJoin<TModelB>(string alias, Expression<Func<TModel, TModelB, bool>> join)
        {
            return Join(alias, join, "right join");
        }
        public FaaastQuery<TModelB> InnerJoin<TModelA, TModelB>(string aliasA, string aliasB, Expression<Func<TModelA, TModelB, bool>> join)
        {
            return Join(aliasB, join, "inner join");
        }

        public FaaastQuery<TModelB> LeftJoin<TModelA, TModelB>(string aliasA, string aliasB, Expression<Func<TModelA, TModelB, bool>> join)
        {
            return Join(aliasB, join, "left join");
        }

        public FaaastQuery<TModelB> RightJoin<TModelA, TModelB>(string aliasA, string aliasB, Expression<Func<TModelA, TModelB, bool>> join)
        {
            return Join(aliasB, join, "right join");
        }

        internal object ConvertValue(UnaryClause clause)
        {

            if (clause is ConstantClause constant)
            {
                return constant.Value;
            }
            else if (clause is PropertyClause property)
            {
                var mapping = Db.Mappings.Value.TypeToMapping[property.ObjectType];
                string alias = property.References?.ToString();
                if (!string.IsNullOrWhiteSpace(alias) && !string.IsNullOrWhiteSpace(LastAlias))
                {
                    return string.Concat(alias, ".", (mapping).PropertyToColumn[property.Property].Name);
                }
                else
                    return Mapping.PropertyToColumn[property.Property].Name;
            }
            else if (clause is OperationClause operation)
            {
                return string.Format(operation.Function, ConvertValue(operation.Clause));
            }

            throw new NotImplementedException();
        }
    }
}
