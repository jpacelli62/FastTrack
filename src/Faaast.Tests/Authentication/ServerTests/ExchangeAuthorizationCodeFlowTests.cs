using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Faaast.Tests.Authentication.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Faaast.Tests.Authentication.ServerTests
{
    public class ExchangeAuthorizationCodeFlowTests : IClassFixture<ServerFixture>
    {
        public ServerFixture Fixture { get; set; }

        public TestServer Server { get; set; }

        public ExchangeAuthorizationCodeFlowTests(ServerFixture fixture)
        {
            this.Fixture = fixture;
            this.Server = fixture.CreateServerApp(null, builder => builder.AddAuthorizationCodeFlow());
        }
        private Task<Transaction> QueryAsync(string clientId, string scopes, string redirectUri, string state) => this.QueryAsync(this.Server, clientId, scopes, redirectUri, state);

        private async Task<Transaction> QueryAsync(TestServer server, string clientId, string clientSecret, string redirectUri, string code)
        {
            var dic = new Dictionary<string, string>
            {
                {"client_id", clientId},
                {"redirect_uri", redirectUri ?? $"https://{this.Fixture.ClientHost}/faaastoauth/signin" },
                {"client_secret", clientSecret},
                {"code", code },
                {"grant_type", "authorization_code" },
            };
            return await ServerUtils.SendPostAsync(server, this.Fixture.TokenEndpoint, dic);
        }

        [Fact]
        public async Task Test_should_not_handle()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, this.Fixture.TokenEndpoint);
            var transaction = await ServerUtils.SendAsync(this.Server, request);
            Assert.Equal(HttpStatusCode.NotFound, transaction.Response.StatusCode);
        }

        [Fact]
        public async Task Test_invalid_client()
        {
            var transaction = await this.QueryAsync("wrongid",
                this.Fixture.Client.ClientSecret,
                null,
                "code");
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_forbidden_flow()
        {
            var transaction = await this.QueryAsync("disallowFlow",
                this.Fixture.Client.ClientSecret,
                null,
                "code");
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_ForbiddenFlow, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_redirecturl()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                "https://donotredirect.com/",
                "code");
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidRedirectUri, transaction.ResponseText);
        }

        [Fact]
        public void Test_invalid_code()
        {
            lock (this.Fixture)
            {
                this.Fixture.Code = null;
                var transaction = this.QueryAsync(this.Fixture.Client.ClientId,
                    this.Fixture.Client.ClientSecret,
                    null,
                    "code").Result;
                Assert.Null(this.Fixture.Code);
                Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
                Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidCode, transaction.ResponseText);
            }
        }

        [Fact]
        public void Test_nominal()
        {
            lock (this.Fixture)
            {
                this.Fixture.Code = null;
                List<Claim> claims = new();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, "123"));
                var principal = new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
                var properties = new AuthenticationProperties
                {
                    IssuedUtc = this.Fixture.Clock.UtcNow,
                    ExpiresUtc = this.Fixture.Clock.UtcNow + TimeSpan.FromMinutes(5)
                };
                properties.SetString("Scope", "identity");
                this.Fixture.Code = new OAuth2Server.AuthorizationCode()
                {
                    Code = Guid.NewGuid().ToString(),
                    Expires = this.Fixture.Clock.UtcNow,
                    Ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, properties, CookieAuthenticationDefaults.AuthenticationScheme)
                };

                var transaction = this.QueryAsync(this.Fixture.Client.ClientId,
                    this.Fixture.Client.ClientSecret,
                    null,
                    this.Fixture.Code.Code).Result;

                Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
                Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidCode, transaction.ResponseText);

                this.Fixture.Code.Expires = this.Fixture.Clock.UtcNow + TimeSpan.FromMinutes(5);
                transaction = this.QueryAsync(this.Fixture.Client.ClientId,
                   this.Fixture.Client.ClientSecret,
                   null,
                   this.Fixture.Code.Code).Result;

                Assert.Equal(HttpStatusCode.OK, transaction.Response.StatusCode);
            }
        }

        [Fact]
        public void Empty_authorizationEndpoint_throws_exception() => Assert.Throws<ArgumentException>(() => this.Fixture.CreateServerApp(options => options.AuthorizeEndpointPath = null, builder => builder.AddAuthorizationCodeFlow()));

        [Fact]
        public void Empty_tokenEndpoint_throws_exception() => Assert.Throws<ArgumentException>(() => this.Fixture.CreateServerApp(options => options.TokenEndpointPath = null, builder => builder.AddAuthorizationCodeFlow()));
    }
}
