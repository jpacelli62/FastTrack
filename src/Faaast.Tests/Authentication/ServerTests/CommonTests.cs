using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Faaast.OAuth2Server;
using Faaast.OAuth2Server.Core;
using Faaast.Tests.Authentication.Utility;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Faaast.Tests.Authentication.ServerTests
{
    public class CommonTests : IClassFixture<ServerFixture>
    {
        public ServerFixture Fixture { get; set; }

        public TestServer Server { get; set; }

        public CommonTests(ServerFixture fixture)
        {
            this.Fixture = fixture;
            this.Server = fixture.CreateServerApp(null, builder => builder.AddClientCredentialsGrant());
        }

        //[Fact]
        //public void Empty_issuer_throws_exception() => Assert.Throws<ArgumentException>(() => this.Fixture.CreateServerApp(options => options.Issuer = null, builder => { }));

        [Fact]
        public async Task Insecure_fails_when_disabled()
        {
            var server = this.Fixture.CreateServerApp(options => options.AllowInsecureHttp = false, builder => builder.AddClientCredentialsGrant());
            var transaction = await ServerUtils.SendPostAsync(server, this.Fixture.TokenEndpoint.Replace("https", "http"), new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", this.Fixture.Client.ClientId},
                { "client_secret", this.Fixture.Client.ClientSecret },
                { "audience", TestClient.Audience }
            });
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_Insecure, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_internal_crash()
        {
            var server = this.Fixture.CreateServerApp(options => { }, builder => builder.AddClientCredentialsGrant());
            var transaction = await ServerUtils.SendPostAsync(server, this.Fixture.TokenEndpoint, new Dictionary<string, string>
            {
                { "grant_type",  "client_credentials" },
                { "client_id","throw" },
                { "client_secret", this.Fixture.Client.ClientSecret },
                { "audience", TestClient.Audience }
            });
            Assert.Equal(HttpStatusCode.InternalServerError, transaction.Response.StatusCode);
            Assert.Equal("ArgumentException : test error message", transaction.ResponseText);
        }

        [Fact]
        public async Task Test_disabled_detailedError_crash()
        {
            var server = this.Fixture.CreateServerApp(options => options.DisplayDetailedErrors = false, builder => builder.AddClientCredentialsGrant());
            var transaction = await ServerUtils.SendPostAsync(server, this.Fixture.TokenEndpoint, new Dictionary<string, string>
            {
                { "grant_type",  "client_credentials" },
                { "client_id","throw" },
                { "client_secret", this.Fixture.Client.ClientSecret },
                { "audience", TestClient.Audience }
            });
            Assert.Equal(HttpStatusCode.InternalServerError, transaction.Response.StatusCode);
            Assert.Equal("internal server error", transaction.ResponseText);
        }

        [Fact]
        public async Task Test_disabled_detailedError_badrequest()
        {
            var server = this.Fixture.CreateServerApp(options => options.DisplayDetailedErrors = false, builder => builder.AddClientCredentialsGrant());
            var transaction = await ServerUtils.SendPostAsync(server, this.Fixture.TokenEndpoint, new Dictionary<string, string>
            {
                { "grant_type",  "client_credentials" },
                { "client_id", "empty" },
                { "client_secret", this.Fixture.Client.ClientSecret },
                { "audience", TestClient.Audience }
            });
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal("bad request", transaction.ResponseText);
        }

        [Fact]
        public async Task Test_missing_required_parameter()
        {
            var server = this.Fixture.CreateServerApp(options => options.AllowInsecureHttp = false, builder => builder.AddClientCredentialsGrant());
            var transaction = await ServerUtils.SendPostAsync(server, this.Fixture.TokenEndpoint, new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            });
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal("The 'client_id' parameter is required with 'POST,GET' method(s)", transaction.ResponseText);
        }

        [Fact]
        public void Test_buildUri_specific_port()
        {
            var uri = OAuth2Server.Core.OAuthMiddleware.BuildUri("https", "mycompany.com", 44310, "/", "");
            Assert.Equal("https://mycompany.com:44310/", uri);
        }

        [Fact]
        public void Test_exception_serialization()
        {
            var ex = new RequestException("param", "get");
            using var stream = new MemoryStream();
#pragma warning disable SYSLIB0011 // Le type ou le membre est obsolète
            new BinaryFormatter().Serialize(stream, ex);
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            var myBinaryFormatter = new BinaryFormatter
            {
                Binder = new RequestExceptionBinder()
            };

            var exClone = (RequestException)myBinaryFormatter.Deserialize(stream);
#pragma warning restore SYSLIB0011 // Le type ou le membre est obsolète
            Assert.Equal(ex.ParameterName, exClone.ParameterName);
            Assert.Equal(ex.ExpectedMethod, exClone.ExpectedMethod);
        }

        private class RequestExceptionBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                if (typeName != "Faaast.OAuth2Server.Core.RequestException")
                {
                    throw new SerializationException("Only RequestException is allowed"); // Compliant
                }

                return typeof(RequestException);
            }
        }
    }
}
