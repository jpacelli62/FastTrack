using Faaast.DatabaseModel;
using Faaast.Metadata;
using Faaast.Orm;
using Faaast.Orm.Reader;
using Faaast.Tests.Orm.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using SqlKata;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Faaast.Tests.Orm
{
    public class ExpressionTests : IClassFixture<FaaastOrmFixture>
    {
        public FaaastOrmFixture Fixture { get; set; }

        public ExpressionTests(FaaastOrmFixture fixture)
        {
            Fixture = fixture;
        }


        [Fact]
        public void Check_Sql_builder()
        {
            Assert.Equal("SELECT * FROM [sampleTable] WHERE [V1] = @p0", BuildSqlQuery<SimpleModel>(x => x.V1 == 1));
            Assert.Equal("SELECT * FROM [sampleTable] WHERE [V8] = @p0", BuildSqlQuery<SimpleModel>(x => x.V8));
            Assert.Equal("SELECT * FROM [sampleTable] WHERE NOT ([V8] = @p0)", BuildSqlQuery<SimpleModel>(x => !x.V8));
            Assert.Equal("SELECT * FROM [sampleTable] WHERE [V3] <> @p0", BuildSqlQuery<SimpleModel>(x => x.V3 != DateTime.Now));

            var sampleObject = new SimpleModel { V3 = DateTime.Now };
            Assert.Equal("SELECT * FROM [sampleTable] WHERE [V3] <> @p0", BuildSqlQuery<SimpleModel>(x => x.V3 != sampleObject.V3));

            Assert.Equal("SELECT * FROM [sampleTable] WHERE ([V1] = @p0 OR [V1] = @p1)", BuildSqlQuery<SimpleModel>(x => x.V1 == 1 || x.V1 == 2));
            Assert.Equal("SELECT * FROM [sampleTable] WHERE (([V2] = @p0 AND ([V1] = @p0 OR [V1] = @p0)) AND [V2] = @p0)", BuildSqlQuery<SimpleModel>(x => x.V2 == "test" && (x.V1 == 1 || x.V1 == 2) && x.V2 == "test2"));

            Assert.Equal("SELECT * FROM [sampleTable] WHERE [V2] LIKE @p0", BuildSqlQuery<SimpleModel>(x => x.V2.Contains("test")));


            Assert.Equal("SELECT * FROM [sampleTable] WHERE [V1] = @p0", BuildSqlQuery<SimpleModel>(x => x.V1 == 1));

            var db = Fixture.GetDb(out var provider);
            var query = db.From<SimpleModel>().OrderBy(x => x.V2.Length);
            Assert.Equal("SELECT * FROM [sampleTable] ORDER BY LEN([V2]) ASC", query.Compile().Sql);

            var query2 = db.From<SimpleModel>("A").InnerJoin<SimpleModel>("B", (A,B) => A.V1 == B.V1);
            var test = query2.Compile();
            Assert.Equal("SELECT [v1], [V2], [V3], [V4], [V5], [V6], [V7], [V8] FROM [sampleTable] ORDER BY LEN([V2]) ASC", test.Sql);

        }



        private string BuildSqlQuery<T>(Expression<Func<T, bool>> exp)
        {
            var db = Fixture.GetDb(out var provider);

            var query = db.From<T>().Where<T>(exp);
            var compiledQuery = query.Compile();
            return compiledQuery.Sql;
        }


    }
}
