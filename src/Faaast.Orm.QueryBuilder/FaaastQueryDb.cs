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

        public TableAlias<T> Alias<T>(string alias = null)
        {
            var mapping = this.Mapping<T>();
            return mapping != null ? new TableAlias<T>(mapping, alias) : null;
        }

        public Query Sql => new FaaastQuery(this);

        public virtual CompiledQuery Compile(Query query)
        {
            var result = this.Compiler.Compile(query);
            return new CompiledQuery(result.Sql, result.NamedBindings);
        }
    }
}
