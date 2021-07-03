using Faaast.DatabaseModel;
using Faaast.Orm.Resolver;
using Faaast.Tests.Metadata;
using Faaast.Tests.Orm.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Faaast.Authentication.OAuth2;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.AspNetCore.Authentication;

namespace Faaast.Tests.Authentication
{
    public class AuthenticationTests
    {
        public IServiceProvider Services { get; set; }

        public AuthenticationTests()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddAuthentication(FacebookDefaults.AuthenticationScheme).AddFacebook(options => {
                options.AppId = "myAppId";
                options.AppSecret = "vcvbcbcvbvcb";
            });
            Services = services.BuildServiceProvider();
        }

        [Fact]
        public void Handler_exists()
        {
            Mock<HttpContext> httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(x => x.RequestServices).Returns(Services);
            var handler = Services.GetRequiredService<FacebookHandler>();

            var Schemes = Services.GetRequiredService<IAuthenticationSchemeProvider>();
            var handlers = Services.GetRequiredService<IAuthenticationHandlerProvider>();
            foreach (var scheme in Schemes.GetRequestHandlerSchemesAsync().Result)
                handler.InitializeAsync(scheme, httpContext.Object);

            var result = handler.AuthenticateAsync().Result;
            Assert.NotNull(handler);

            //var definition = SampleTable.Get(Meta.PocoObject);
            //Assert.NotNull(definition);

            //Assert.True(SampleTable.Columns.Where(x => x.Key != nameof(SampleModelDto.IntMember)).All(x => x.Value.Identity == false && x.Value.PrimaryKey == false));
            //Assert.True(SampleTable.Columns[nameof(SampleModelDto.IntMember)].Identity && SampleTable.Columns[nameof(SampleModelDto.IntMember)].PrimaryKey);

            //Assert.Equal("dbo", SampleTable.Columns[nameof(SampleModelDto.ReadWriteProperty)].Get(DbMeta.ReferenceSchema));
            //Assert.Equal("pktable", SampleTable.Columns[nameof(SampleModelDto.ReadWriteProperty)].Get(DbMeta.ReferenceTable));
            //Assert.Equal("pkcolumn", SampleTable.Columns[nameof(SampleModelDto.ReadWriteProperty)].Get(DbMeta.ReferenceColumn));

            //Assert.Equal(50, SampleTable.Columns[nameof(SampleModelDto.ReadWriteProperty)].Get(DbMeta.Length));

            //var pk = SampleTable.PrimaryKeyColumns();
            //Assert.True(pk.Length == 1);
            //Assert.Equal(nameof(SampleModelDto.IntMember), pk[0].Name);
        }
    }
}
