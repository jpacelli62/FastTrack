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

        public FaaastQuery<TClass> From<TClass>(string alias = null) => this.Sql.From<TClass>(alias);

        public FaaastQuery Sql => new FaaastQuery(this);

        public virtual CompiledQuery Compile(FaaastQuery query) => this.Compile(query.Query);

        public virtual CompiledQuery Compile(Query query)
        {
            var result = this.Compiler.Compile(query);
            return new CompiledQuery(result.Sql, result.NamedBindings);
        }
    }
}
