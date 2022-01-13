using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Faaast.Tests.Authentication.Utility;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Faaast.Tests.Authentication.ServerTests
{
    public class ClientCredentialsGrantTests : IClassFixture<ServerFixture>
    {
        public ServerFixture Fixture { get; set; }

        public CustomTestServer Server { get; set; }

        public ClientCredentialsGrantTests(ServerFixture fixture)
        {
            this.Fixture = fixture;
            this.Server = fixture.CreateServer(builder => builder.AddClientCredentialsGrantFlow());
        }

        private async Task<Transaction> QueryAsync(string clientId, string clientSecret, string audience) =>
            await this.Server.SendPostAsync(this.Fixture.TokenEndpoint, new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId},
                { "client_secret", clientSecret },
                { "audience", audience }
            });

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
            var transaction = await this.QueryAsync("wrongid", this.Fixture.Client.ClientSecret, this.Fixture.Client.Audience);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_password()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId, "wrongpassword", this.Fixture.Client.Audience);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_audience()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId, this.Fixture.Client.ClientSecret, "wrongaudience");
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidAudience, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_nominal()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId, this.Fixture.Client.ClientSecret, this.Fixture.Client.Audience);
            Assert.Equal(HttpStatusCode.OK, transaction.Response.StatusCode);

            var payload = this.Fixture.Read(transaction.ResponseText, false);
            Assert.NotNull(payload);
            Assert.Equal(this.Fixture.Client.Audience, payload["aud"]?.ToString());
            Assert.Equal(this.Fixture.Client.ClientId, payload["role"]?.ToString());
            Assert.Equal("-1", payload["nameid"]?.ToString());
        }

        [Fact]
        public void Empty_tokenEndpoint_throws_exception() => Assert.Throws<ArgumentException>(() => this.Fixture.CreateServer(builder => builder.AddClientCredentialsGrantFlow(), options => options.TokenEndpointPath = null));
    }
}
