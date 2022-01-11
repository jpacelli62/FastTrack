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
        public async Task Test_invalid_code()
        {
            var fixture = new ServerFixture();
            var server = fixture.CreateServerApp(null, builder => builder.AddAuthorizationCodeFlow());

            var transaction = await this.QueryAsync(server, fixture.Client.ClientId,
                fixture.Client.ClientSecret,
                null,
                "code");
            Assert.Null(fixture.Code);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidCode, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_nominal()
        {
            var fixture = new ServerFixture();
            var server = fixture.CreateServerApp(null, builder => builder.AddAuthorizationCodeFlow());

            List<Claim> claims = new();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, "123"));
            var principal = new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            var properties = new AuthenticationProperties
            {
                IssuedUtc = fixture.Clock.UtcNow,
                ExpiresUtc = fixture.Clock.UtcNow + TimeSpan.FromMinutes(5)
            };
            properties.SetString("Scope", "identity");
            fixture.Code = new OAuth2Server.AuthorizationCode()
            {
                Code = Guid.NewGuid().ToString(),
                Expires = fixture.Clock.UtcNow,
                Ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, properties, CookieAuthenticationDefaults.AuthenticationScheme)
            };

            var transaction = await this.QueryAsync(server, fixture.Client.ClientId,
                fixture.Client.ClientSecret,
                null,
                fixture.Code.Code);

            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidCode, transaction.ResponseText);

            fixture.Code.Expires = fixture.Clock.UtcNow + TimeSpan.FromMinutes(5);
            transaction = await this.QueryAsync(server, fixture.Client.ClientId,
                fixture.Client.ClientSecret,
                null,
                fixture.Code.Code);

            Assert.Equal(HttpStatusCode.OK, transaction.Response.StatusCode);
        }

        [Fact]
        public void Empty_authorizationEndpoint_throws_exception() => Assert.Throws<ArgumentException>(() => this.Fixture.CreateServerApp(options => options.AuthorizeEndpointPath = null, builder => builder.AddAuthorizationCodeFlow()));

        [Fact]
        public void Empty_tokenEndpoint_throws_exception() => Assert.Throws<ArgumentException>(() => this.Fixture.CreateServerApp(options => options.TokenEndpointPath = null, builder => builder.AddAuthorizationCodeFlow()));
    }
}
