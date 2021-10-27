using System;
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
            return new FaaastQuery<TClass>(this).From<TClass>(alias);
        }

        public CompiledQuery Compile(FaaastQuery query)
        {
            var result = Compiler.Compile(query);
            return new CompiledQuery(result.Sql, result.NamedBindings);
        }
    }
}
