using System;
using System.Collections.Generic;
using Faaast.Orm;
using Faaast.Orm.Converters;
using Faaast.Orm.Model;
using Faaast.Tests.Orm.FakeDb;

namespace Faaast.Tests.Orm.Fixture
{
    public class FakeDB : FaaastDb
    {
        public FakeDB(IServiceProvider provider) : base(provider)
        {
            var data = new Dictionary<string, object>()
            {
                { "v1", 123 },
                { "V2", "lorem ipsum"},
                { "V3", DateTime.Today},
                { "V4", Guid.NewGuid()},
                { "V5", 3.14f},
                { "V6", 156651L},
                { "V7", 3.141592653d},
                { "V8", true}
            };

            this.Engine = new FakeEngine()
            {
                FakeConnection = new FakeDbConnection(data, 1000)
            };

            this.ConnectionOverride = new("connectionName", this.Engine, "sampleConnectionString");
        }

        public override ConnectionSettings Connection => this.ConnectionOverride;

        public FakeEngine Engine { get; set; } 

        public ConnectionSettings ConnectionOverride { get; set; }

        protected override IEnumerable<SimpleTypeMapping> LoadMappings()
        {
            var mapping = new SimpleTypeMapping<SimpleModel>();
            mapping.ToDatabase("MyDb");
            mapping.ToTable("sampleTable", "dbo");
            mapping.Map(x => x.V1, "v1").IsPrimaryKey().IsIdentity();
            mapping.Map(x => x.V2, "V2").Length(100).References("otherschema", "othertable", "othercolumn");
            mapping.Map(x => x.V3, "V3");
            mapping.Map(x => x.V4, "V4").IsComputed();
            mapping.Map(x => x.V5, "V5").IsNullable();
            mapping.Map(x => x.V6, "V6");
            mapping.Map(x => x.V7, "V7");
            mapping.Map(x => x.V8, "V8");
            mapping.Map(x => x.EnumValue, "State").Converter<EnumToIntValueConverter<TestState>>();
            yield return mapping;
        }
    }
}
