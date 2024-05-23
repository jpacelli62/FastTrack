using System;
using SqlKata;
using SqlKata.Compilers;

namespace Faaast.Orm
{
    public abstract class FaaastQueryDb : FaaastDb
    {
        public abstract Compiler Compiler { get; }

        protected FaaastQueryDb(IServiceProvider services) : base(services)
        {
        }

        public Query Sql => new FaaastQuery(this);

        public virtual CompiledQuery Compile(Query query)
        {
            var result = this.Compiler.Compile(query);
            return new CompiledQuery(result.Sql, result.NamedBindings);
        }
    }
}
