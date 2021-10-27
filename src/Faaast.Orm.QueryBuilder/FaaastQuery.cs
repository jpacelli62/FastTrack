using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SqlKata;

namespace Faaast.Orm
{
    public class FaaastQuery : Query
    {
        public FaaastQueryDb Db { get; set; }

        public FaaastQuery(FaaastQueryDb db)
        {
            this.Db = db;
        }

        public override Query Clone()
        {
            FaaastQuery query = (FaaastQuery)base.Clone();
            query.Db = this.Db;
            return query;
        }

        public override Query NewQuery()
        {
            return new FaaastQuery(Db);
        }

        public FaaastQuery<T> Clone<T>()
        {
            FaaastQuery<T> val = new FaaastQuery<T>(Db);
            List<SqlKata.AbstractClause> newClauses = new();
            foreach (var clause in base.Clauses)
            {
                newClauses.Add(clause.Clone());
            }
            val.Clauses = newClauses;

            val.Parent = Parent;
            val.QueryAlias = QueryAlias;
            val.IsDistinct = IsDistinct;
            val.Method = Method;
            val.Includes = Includes;
            val.Variables = Variables;
            val.Db = this.Db;

            return val;
        }

        public CompiledQuery Compile()
        {

            return Db.Compile(this);

        }
    }

    public class FaaastQuery<TModel> : FaaastQuery
    {
        public FaaastQuery(FaaastQueryDb db) : base(db)
        {
        }

        public FaaastQuery<TModelB> InnerJoin<TModelB>(string alias, Expression<Func<TModel, TModelB, bool>> join)
        {
            FaaastQuery<TModelB> clone = Clone<TModelB>();
            var mapping = Db.Mapping<TModelB>();
            string tableName = mapping.Table.Name;
            AbstractClause clause = TreeExtensions.VisitExpression(join);
            if(clause is BinaryColumnClause binary)
            {
                if(binary.Left is PropertyClause left && binary.Right is PropertyClause right)
                {
                    clone.Join(string.Concat(tableName, " AS ", alias), q => q.On(
                        FormatName(mapping.PropertyToColumn[left.Property].Name, this.QueryAlias),
                        FormatName(mapping.PropertyToColumn[right.Property].Name, alias),
                       binary.Operation));
                }
            }
            return clone;
        }

        internal string FormatName(string property, string alias)
        {
            if (!string.IsNullOrWhiteSpace(alias))
            {
                return string.Concat(alias, ".", property);
            } else { return property; }
        }
    }
}
