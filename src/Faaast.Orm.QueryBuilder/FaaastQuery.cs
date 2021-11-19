using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Faaast.Orm.Mapping;
using SqlKata;

namespace Faaast.Orm
{
    public class FaaastQuery : FaaastQueryBase<FaaastQuery>
    {
        public FaaastQuery(FaaastQueryDb db) : base(db)
        {
        }

        public override FaaastQuery NewQuery() => new(base.Db);

        public CompiledQuery Compile() => base.Db.Compile(this);

        public FaaastQuery<TModel> Clone<TModel>()
        {
            var clone = new FaaastQuery<TModel>(this.Db)
            {
                Query = this.Query,
                LastAlias = this.Query.QueryAlias
            };
            return clone;
        }

        public FaaastQuery<TClass> From<TClass>(string alias = null)
        {
            var clone = this.Clone<TClass>();
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

        public FaaastQuery(FaaastQueryDb db) : base(db) => this.Mapping = this.Db.Mapping<TModel>();

        public FaaastQuery<TModel> OrderBy<TProperty>(Expression<Func<TModel, TProperty>> exp, string order = "ASC")
        {
            var clause = TreeExtensions.VisitExpression(exp);

            if (clause is PropertyClause prop)
            {
                this.Query.OrderBy(this.Mapping.PropertyToColumn[prop.Property].Name);
            }
            else if (clause is OperationClause op)
            {
                this.Query.OrderByRaw(string.Concat(string.Format(op.Function, this.Mapping.PropertyToColumn[op.Clause.Property].Name), " ASC"));
            }
            else
            {
                throw new NotImplementedException();
            }

            return this;
        }

        public FaaastQuery<TModel> OrderByDescending<TProperty>(Expression<Func<TModel, TProperty>> exp)
        {
            var clause = TreeExtensions.VisitExpression(exp);
            if (clause is PropertyClause prop)
            {
                this.Query.OrderByDesc(this.Mapping.PropertyToColumn[prop.Property].Name);
            }
            else if (clause is OperationClause op)
            {
                this.Query.OrderByRaw(string.Concat(string.Format(op.Function, this.Mapping.PropertyToColumn[op.Clause.Property].Name), " DESC"));
            }
            else
            {
                throw new NotImplementedException();
            }

            return this;
        }

        public FaaastQuery<TModel> Top(int nb)
        {
            this.Query.Limit(nb);
            return this;
        }

        public FaaastQuery<TModel> Where(Dictionary<string, object> keyValuePairs)
        {
            this.Query.Where(keyValuePairs);
            return this;
        }

        internal FaaastQuery<TModel> AsInsert(Dictionary<string, object> insert, bool returnId)
        {
            this.Query.AsInsert(insert, returnId);
            return this;
        }

        public FaaastQuery<TModel> AsUpdate(Dictionary<string, object> update)
        {
            this.Query.AsUpdate(update);
            return this;
        }

        public FaaastQuery<TModel> AsDelete()
        {
            this.Query.AsDelete();
            return this;
        }

        public FaaastQuery<TModel> SelectFields()
        {
            if (!string.IsNullOrWhiteSpace(this.LastAlias))
            {
                foreach (var map in this.Mapping.ColumnMappings)
                {
                    this.Query.Select(string.Concat(this.LastAlias, ".", map.Column.Name));
                }
            }
            else
            {
                foreach (var map in this.Mapping.ColumnMappings)
                {
                    this.Query.Select(map.Column.Name);
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
                    this.Query.Select(this.ConvertValue(property).ToString());
                }
            }

            return this;
        }

        public FaaastQuery<T> Select<T>(string alias)
        {
            var mapping = this.Db.Mapping<T>();
            if (!string.IsNullOrWhiteSpace(alias))
            {
                foreach (var map in mapping.ColumnMappings)
                {
                    this.Query.Select(string.Concat(alias, ".", map.Column.Name));
                }
            }
            else
            {
                foreach (var map in mapping.ColumnMappings)
                {
                    this.Query.Select(map.Column.Name);
                }
            }

            return this.Clone<T>();
        }

        public FaaastQuery<TModel> Where(Expression<Func<TModel, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }

        public FaaastQuery<TModel> Where<A>(Expression<Func<A, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B>(Expression<Func<A, B, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C>(Expression<Func<A, B, C, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D>(Expression<Func<A, B, C, D, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E>(Expression<Func<A, B, C, D, E, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F>(Expression<Func<A, B, C, D, E, F, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G>(Expression<Func<A, B, C, D, E, F, G, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H>(Expression<Func<A, B, C, D, E, F, G, H, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I>(Expression<Func<A, B, C, D, E, F, G, H, I, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I, J>(Expression<Func<A, B, C, D, E, F, G, H, I, J, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }

        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I, J, K>(Expression<Func<A, B, C, D, E, F, G, H, I, J, K, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I, J, K, L>(Expression<Func<A, B, C, D, E, F, G, H, I, J, K, L, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I, J, K, L, M>(Expression<Func<A, B, C, D, E, F, G, H, I, J, K, L, M, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I, J, K, L, M, N>(Expression<Func<A, B, C, D, E, F, G, H, I, J, K, L, M, N, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }
        public FaaastQuery<TModel> Where<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O>(Expression<Func<A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, bool>> exp)
        {
            var result = TreeExtensions.VisitExpression(exp);
            this.ConvertWhere(result);
            return this;
        }

        internal void ConvertWhere(AbstractClause clause, Query q = null)
        {
            if (clause is BinaryColumnClause binary)
            {
                if (binary.Left is UnaryClause leftUnary && binary.Right is UnaryClause rightUnary)
                {
                    (q ?? this.Query).Where(this.ConvertValue(leftUnary).ToString(), binary.Operation, this.ConvertValue(rightUnary));
                    return;
                }
                else
                {
                    (q ?? this.Query).Where(q =>
                    {
                        this.ConvertWhere(binary.Left, q);
                        if (string.Compare("or", binary.Operation, true) == 0)
                        {
                            q.Or();
                        }

                        this.ConvertWhere(binary.Right, q);
                        return q;
                    });
                    return;
                }
            }
            else if (clause is PropertyClause property)// Implicit true
            {
                (q ?? this.Query).Where(this.ConvertValue(property).ToString(), "=", 1);
                return;
            }
            else if (clause is NegateClause negate)
            {
                (q ?? this.Query).Not();
                this.ConvertWhere(negate.Clause, q);
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
                    join.On(this.ConvertValue(leftUnary).ToString(), this.ConvertValue(rightUnary).ToString(), binary.Operation);
                    return;
                }
                else
                {
                    this.ConvertJoin(binary.Left, join);
                    if (string.Compare("or", binary.Operation, true) == 0)
                    {
                        join.Or();
                    }

                    this.ConvertJoin(binary.Right, join);
                    return;
                }
            }
            else if (clause is PropertyClause property)// Implicit true
            {
                join.AddComponent("where", new BasicCondition
                {
                    Column = this.ConvertValue(property).ToString(),
                    Operator = "=",
                    Value = 1,
                    IsOr = false,
                    IsNot = false,
                });
                return;
            }
            else if (clause is NegateClause negate)
            {
                this.Query.Not();
                this.ConvertJoin(negate.Clause, join);
                return;
            }

            throw new NotImplementedException();
        }
        private FaaastQuery<TModelB> Join<TModelA, TModelB>(string alias, Expression<Func<TModelA, TModelB, bool>> join, string type)
        {
            var clone = this.Clone<TModelB>();
            clone.LastAlias = alias;
            //join.Parameters.
            var clause = TreeExtensions.VisitExpression(join);
            clone.Query.Join($"{clone.Mapping.Table.Name} AS {alias}", q =>
            {
                this.ConvertJoin(clause, q);
                return q;
            }, type);

            return clone;
        }

        public FaaastQuery<TModelB> InnerJoin<TModelB>(string alias, Expression<Func<TModel, TModelB, bool>> join) => this.Join(alias, join, "inner join");
        public FaaastQuery<TModelB> InnerJoin<TModelA, TModelB>(string aliasA, string aliasB, Expression<Func<TModelA, TModelB, bool>> join) => this.Join(aliasB, join, "inner join");

        public FaaastQuery<TModelB> LeftJoin<TModelB>(string alias, Expression<Func<TModel, TModelB, bool>> join) => this.Join(alias, join, "left join");
        public FaaastQuery<TModelB> LeftJoin<TModelA, TModelB>(string aliasA, string aliasB, Expression<Func<TModelA, TModelB, bool>> join) => this.Join(aliasB, join, "left join");

        public FaaastQuery<TModelB> RightJoin<TModelB>(string alias, Expression<Func<TModel, TModelB, bool>> join) => this.Join(alias, join, "right join");

        public FaaastQuery<TModelB> RightJoin<TModelA, TModelB>(string aliasA, string aliasB, Expression<Func<TModelA, TModelB, bool>> join) => this.Join(aliasB, join, "right join");

        internal object ConvertValue(UnaryClause clause)
        {

            if (clause is ConstantClause constant)
            {
                return constant.Value;
            }
            else if (clause is PropertyClause property)
            {
                var mapping = this.Db.Mappings.Value.TypeToMapping[property.ObjectType];
                var alias = property.References?.ToString();
                return !string.IsNullOrWhiteSpace(alias) && !string.IsNullOrWhiteSpace(this.LastAlias)
                    ? string.Concat(alias, ".", mapping.PropertyToColumn[property.Property].Name)
                    : this.Mapping.PropertyToColumn[property.Property].Name;
            }
            else if (clause is OperationClause operation)
            {
                return string.Format(operation.Function, this.ConvertValue(operation.Clause));
            }

            throw new NotImplementedException();
        }
    }
}
