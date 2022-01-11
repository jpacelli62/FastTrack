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

        public TestServer Server { get; set; }

        public ResourceOwnerPasswordCredentialsFlowTests(ServerFixture fixture)
        {
            this.Fixture = fixture;
            this.Server = fixture.CreateServerApp(null, builder => builder.AddResourceOwnerPasswordCredentialsFlow());
        }

        private async Task<Transaction> QueryAsync(string clientId, string clientSecret, string username, string password, string audience, string scope)
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

            return await ServerUtils.SendPostAsync(this.Server, this.Fixture.TokenEndpoint, parameters);
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
                this.Fixture.Client.Username,
                this.Fixture.Client.Password,
                TestClient.Audience,
                TestClient.Scope);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_clientSecret()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId,
                "wrong",
                this.Fixture.Client.Username,
                this.Fixture.Client.Password,
                TestClient.Audience,
                TestClient.Scope);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_username()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                "wrong",
                this.Fixture.Client.Password,
                TestClient.Audience,
                TestClient.Scope);
            Assert.Equal(HttpStatusCode.Forbidden, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_LoginFailed, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_password()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                 this.Fixture.Client.Username,
               "wrong",
                TestClient.Audience,
                TestClient.Scope);
            Assert.Equal(HttpStatusCode.Forbidden, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_LoginFailed, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_audience()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                 this.Fixture.Client.Username,
                this.Fixture.Client.Password,
                "wrong",
                TestClient.Scope);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidAudience, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_scope()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                 this.Fixture.Client.Username,
                this.Fixture.Client.Password,
                TestClient.Audience,
                "wrong");
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidScope, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_client_login_fail_with_message()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                "lockedtest",
                this.Fixture.Client.Password,
                TestClient.Audience,
                TestClient.Scope);
            Assert.Equal(HttpStatusCode.Forbidden, transaction.Response.StatusCode);
            Assert.Equal("Locked account", transaction.ResponseText);
        }

        [Fact]
        public async Task Test_forbidden_flow()
        {
            var transaction = await this.QueryAsync("disallowFlow",
                this.Fixture.Client.ClientSecret,
                this.Fixture.Client.Username,
                this.Fixture.Client.Password,
                TestClient.Audience,
                TestClient.Scope);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_ForbiddenFlow, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_nominal()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId,
                this.Fixture.Client.ClientSecret,
                this.Fixture.Client.Username,
                this.Fixture.Client.Password,
                TestClient.Audience,
                TestClient.Scope);
            Assert.True(transaction.Response.IsSuccessStatusCode);
            Assert.NotNull(this.Fixture.Token);
            Assert.Equal(this.Fixture.Clock.UtcNow.DateTime + TimeSpan.FromMinutes(10), this.Fixture.Token.AccessTokenExpiresUtc);
            Assert.Equal(this.Fixture.Clock.UtcNow.DateTime + TimeSpan.FromMinutes(100), this.Fixture.Token.RefreshTokenExpiresUtc);
            Assert.NotNull(this.Fixture.Token.AccessToken);
            Assert.NotNull(this.Fixture.Token.RefreshToken);
            Assert.NotNull(this.Fixture.Token.NameIdentifier);
        }
        [Fact]
        public async Task Test_clientCredentials_in_header()
        {
            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", this.Fixture.Client.Username },
                { "password", this.Fixture.Client.Password },
                { "audience", TestClient.Audience },
                { "scope", TestClient.Scope }
            };

            var authorization = $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Concat(this.Fixture.Client.ClientId, ":", this.Fixture.Client.ClientSecret)))}";
            var transaction = await ServerUtils.SendPostAsync(this.Server, this.Fixture.TokenEndpoint, parameters, request => request.Headers.Add("Authorization", authorization));

            Assert.True(transaction.Response.IsSuccessStatusCode);
            Assert.NotNull(this.Fixture.Token);
            Assert.Equal(this.Fixture.Clock.UtcNow.DateTime + TimeSpan.FromMinutes(10), this.Fixture.Token.AccessTokenExpiresUtc);
            Assert.Equal(this.Fixture.Clock.UtcNow.DateTime + TimeSpan.FromMinutes(100), this.Fixture.Token.RefreshTokenExpiresUtc);
            Assert.NotNull(this.Fixture.Token.AccessToken);
            Assert.NotNull(this.Fixture.Token.RefreshToken);
            Assert.NotNull(this.Fixture.Token.NameIdentifier);
        }

        [Fact]
        public void Empty_tokenEndpoint_throws_exception() => Assert.Throws<ArgumentException>(() => this.Fixture.CreateServerApp(options => options.TokenEndpointPath = null, builder => builder.AddResourceOwnerPasswordCredentialsFlow()));
    }
}
