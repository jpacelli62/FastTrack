using System;
using System.Collections.Generic;
using System.Linq;
using Faaast.Orm;
using Faaast.Orm.Model;
using Faaast.Tests.Orm.FakeConnection;
using Faaast.Tests.Orm.Fixtures;

namespace Faaast.Tests.Orm.Fake
{
    public class FakeDB : FaaastDb
    {
        public FakeDB(IServiceProvider provider) : base(provider)
        {
            this.Engine = new FakeEngine()
            {
                FakeConnection = new FakeDbConnection
                {
                    Command = new FakeCommand
                    {
                        Reader = new FakeDataReader()
                        {
                            columns = this.LoadMappings().First().Table.Table.Columns.Select(x=>x.Name).ToList()
                        }
                    }
                }
            };

            this.ConnectionOverride = new("connectionName", this.Engine, "sampleConnectionString");
        }

        public FakeDataReader Data { get => this.Engine.FakeConnection.Command.Reader; }

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
            yield return mapping;
        }
    }
}
