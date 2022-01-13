using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Faaast.Tests.Authentication.Utility;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Faaast.Tests.Authentication.ServerTests
{
    public class ResourceOwnerPasswordCredentialsFlowTests : IClassFixture<ServerFixture>
    {
        public ServerFixture Fixture { get; set; }

        public CustomTestServer Server { get; set; }

        public ResourceOwnerPasswordCredentialsFlowTests(ServerFixture fixture)
        {
            this.Fixture = fixture;
            this.Server = fixture.CreateServer(builder => builder.AddResourceOwnerPasswordCredentialsGrantFlow());
        }

        private async Task<Transaction> QueryAsync(CustomTestServer server, string clientId, string clientSecret, string username, string password, string audience, string scope, Action<HttpRequestMessage> req = null)
        {
            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", clientId},
                { "client_secret", clientSecret },
                { "username", username },
                { "password", password },
                { "audience", audience },
                { "scope", scope }
            };

            return await server.SendPostAsync(this.Fixture.TokenEndpoint, parameters, req);
        }
        private static void DisabledFlow(HttpRequestMessage req) => req.Headers.Add("IsAllowedFlow", "0");

        private static void Behaviour(HttpRequestMessage req, string name) => req.Headers.Add("PasswordSignInAsync", name);

        [Fact]
        public async Task Test_should_not_handle()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, this.Fixture.TokenEndpoint);
            var transaction = await this.Server.SendAsync(request);
            Assert.Equal(HttpStatusCode.NotFound, transaction.Response.StatusCode);
        }

        [Fact]
        public async Task Test_invalid_client()
        {
            var transaction = await this.QueryAsync(
                this.Server,
                "wrongid",
                this.Fixture.Client.ClientSecret,
                "Login",
                "Password",
                this.Fixture.Client.Audience,
                this.Fixture.Client.Scope);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_clientSecret()
        {
            var transaction = await this.QueryAsync(
                this.Server, 
                this.Fixture.Client.ClientId,
                "wrong",
                "Login",
                "Password",
                this.Fixture.Client.Audience,
                this.Fixture.Client.Scope);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_username()
        {
            var transaction = await this.QueryAsync(
                this.Server, 
                this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                "wrong",
                "Password",
                this.Fixture.Client.Audience,
                this.Fixture.Client.Scope);
            Assert.Equal(HttpStatusCode.Forbidden, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_LoginFailed, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_password()
        {
            var transaction = await this.QueryAsync(
                this.Server, 
                this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                 "Login",
               "wrong",
                this.Fixture.Client.Audience,
                this.Fixture.Client.Scope);
            Assert.Equal(HttpStatusCode.Forbidden, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_LoginFailed, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_audience()
        {
            var transaction = await this.QueryAsync(
                this.Server, 
                this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                 "Login",
                "Password",
                "wrong",
                this.Fixture.Client.Scope);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidAudience, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_scope()
        {
            var transaction = await this.QueryAsync(
                this.Server, 
                this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                 "Login",
                "Password",
                this.Fixture.Client.Audience,
                "wrong",
                req => Behaviour(req, "logged"));
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidScope, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_client_login_fail_with_message()
        {
            var transaction = await this.QueryAsync(
                this.Server, 
                this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                "lockedtest",
                "Password",
                this.Fixture.Client.Audience,
                this.Fixture.Client.Scope,
                req => Behaviour(req, "locked"));
            Assert.Equal(HttpStatusCode.Forbidden, transaction.Response.StatusCode);
            Assert.Equal("Locked account", transaction.ResponseText);
        }

        [Fact]
        public async Task Test_forbidden_flow()
        {
            var transaction = await this.QueryAsync(
                this.Server,
                this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                "Login",
                "Password",
                this.Fixture.Client.Audience,
                this.Fixture.Client.Scope,
                req => DisabledFlow(req));
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_ForbiddenFlow, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_nominal()
        {
            var transaction = await this.QueryAsync(
                this.Server, 
                this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                "Login",
                "Password",
                this.Fixture.Client.Audience,
                this.Fixture.Client.Scope,
                req => Behaviour(req, "logged"));
            Assert.True(transaction.Response.IsSuccessStatusCode);
            Assert.NotNull(this.Fixture.Token);
            Assert.Equal(this.Server.Clock.UtcNow.DateTime + TimeSpan.FromMinutes(10), this.Fixture.Token.AccessTokenExpiresUtc);
            Assert.Equal(this.Server.Clock.UtcNow.DateTime + TimeSpan.FromMinutes(100), this.Fixture.Token.RefreshTokenExpiresUtc);
            Assert.NotNull(this.Fixture.Token.AccessToken);
            Assert.NotNull(this.Fixture.Token.RefreshToken);
            Assert.NotNull(this.Fixture.Token.NameIdentifier);

            var payload = this.Fixture.Read(transaction.ResponseText, true);
            Assert.NotNull(payload);
            Assert.Equal("John Doe", payload["unique_name"]?.ToString());
            Assert.Equal("123", payload["nameid"]?.ToString());
            Assert.Equal(this.Fixture.Client.Scope, payload["scope"]?.ToString());
            Assert.Equal(this.Fixture.Client.Audience, payload["aud"]?.ToString());
        }

        [Fact]
        public void Empty_tokenEndpoint_throws_exception() => Assert.Throws<ArgumentException>(() => this.Fixture.CreateServer(builder => builder.AddResourceOwnerPasswordCredentialsGrantFlow(), options => options.TokenEndpointPath = null));
    }
}
