using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Faaast.Orm;
using Faaast.Orm.Model;
using Faaast.Tests.Orm.Fixtures;
using SqlKata.Compilers;

namespace Faaast.Tests.Orm.Fake
{
    public class FakeDB : FaaastQueryDb
    {
        Compiler _compiler;
        public FakeDB(IServiceProvider provider) : base(provider) => _compiler = new SqlServerCompiler();

        protected override Compiler Compiler => _compiler;

        public void SetCompiler(Compiler instance) => this._compiler = instance;

        public override ConnectionSettings Connection => new("connectionName", null, "sampleConnectionString");

        protected override IEnumerable<SimpleTypeMapping> GetMappings()
        {
            var mapping = new SimpleTypeMapping<SimpleModel>();
            mapping.ToDatabase("MyDb");
            mapping.ToTable("sampleTable", "dbo");
            mapping.Map(x => x.V1, "v1").IsPrimaryKey().IsIdentity();
            mapping.Map(x => x.V2, "V2");
            mapping.Map(x => x.V3, "V3");
            mapping.Map(x => x.V4, "V4");
            mapping.Map(x => x.V5, "V5");
            mapping.Map(x => x.V6, "V6");
            mapping.Map(x => x.V7, "V7");
            mapping.Map(x => x.V8, "V8");
            yield return mapping;
        }
    }
}
