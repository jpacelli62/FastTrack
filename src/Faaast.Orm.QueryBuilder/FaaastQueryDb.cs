using System;
using SqlKata;
using SqlKata.Compilers;

namespace Faaast.Orm
{
    public abstract class FaaastQueryDb : FaaastDb
    {
        protected abstract Compiler Compiler { get; }

        protected FaaastQueryDb(IServiceProvider services) : base(services)
        {
        }

        public FaaastQuery<TClass> From<TClass>(string alias = null)
        {
            return Sql.From<TClass>(alias);
        }

        public FaaastQuery Sql
        {
            get
            {
                return new FaaastQuery(this);
            }
        }

        public virtual CompiledQuery Compile(FaaastQuery query)
        {
            return Compile(query.Query);
        }

        public virtual CompiledQuery Compile(Query query)
        {
            var result = Compiler.Compile(query);
            return new CompiledQuery(result.Sql, result.NamedBindings);
        }
    }
}
