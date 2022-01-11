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
    public class ChallengeAuthorizationCodeFlowTests : IClassFixture<ServerFixture>
    {
        public ServerFixture Fixture { get; set; }

        public TestServer Server { get; set; }

        public ChallengeAuthorizationCodeFlowTests(ServerFixture fixture)
        {
            this.Fixture = fixture;
            this.Server = fixture.CreateServerApp(null, builder => builder.AddAuthorizationCodeFlow());
        }
        private Task<Transaction> QueryAsync(string clientId, string scopes, string redirectUri, bool state, bool authenticated = false) => this.QueryAsync(this.Server, clientId, scopes, redirectUri, state, authenticated);

        private async Task<Transaction> QueryAsync(TestServer server, string clientId, string scopes, string redirectUri, bool state, bool authenticated = false)
        {
            var dic = new Dictionary<string, string>
            {
                {"client_id", clientId},
                {"scope", scopes },
                {"response_type", "code" },
                {"redirect_uri", redirectUri ?? $"https://{this.Fixture.ClientHost}/faaastoauth/signin" },
                { "state", state ? "CfDJ8Np1eFHOzoVJni6nVfHZpxxtPlOOOHr8csuyU7jfcKYoseFfn7kHq_e1yKbTVbDqvDoMNPIaoB0emAX8DhXQc7eOyIzHsYZYxwcsDLzcQIdrAMVre16lL2ni2c2F7s_6lY2p136sPyBtUi503YOndrnaKp6j3rlb" : string.Empty}
            };
            return await ServerUtils.SendGetAsync(server, this.Fixture.AuthorizeEndpoint, dic, request =>
            {
                if (authenticated)
                {
                    request.Headers.Add("fakeLoggedUser", "1");
                }
            });
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
                TestClient.Scope,
                null,
                true);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_forbidden_flow()
        {
            var transaction = await this.QueryAsync("disallowFlow",
                TestClient.Scope,
                null,
                true);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_ForbiddenFlow, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_invalid_redirecturl()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId,
                TestClient.Scope,
                "https://donotredirect.com/",
                true);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidRedirectUri, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_notAuthenticated_redirectToLogin()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId,
                TestClient.Scope,
                null,
                true);

            Assert.Equal(HttpStatusCode.Redirect, transaction.Response.StatusCode);
            var redirectPath = transaction.Response.Headers.Location.OriginalString;
            var uri = new UriBuilder(redirectPath);
            Assert.Equal(this.Fixture.ServerHost, uri.Host);
            Assert.Equal("/login", uri.Path);
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
            Assert.Single(queryDictionary);
            Assert.Equal(
                $"https://sso.mycompany.com/oauth/authorize?client_id=MyClientId&scope=identity&response_type=code&redirect_uri=https://{this.Fixture.ClientHost}/faaastoauth/signin&state=CfDJ8Np1eFHOzoVJni6nVfHZpxxtPlOOOHr8csuyU7jfcKYoseFfn7kHq_e1yKbTVbDqvDoMNPIaoB0emAX8DhXQc7eOyIzHsYZYxwcsDLzcQIdrAMVre16lL2ni2c2F7s_6lY2p136sPyBtUi503YOndrnaKp6j3rlb",
                System.Web.HttpUtility.UrlDecode(queryDictionary["returnUrl"]));
        }

        [Fact]
        public async Task Test_invalidScope_toUserConsent()
        {
            var transaction = await this.QueryAsync(this.Fixture.Client.ClientId,
                "wrongScope",
                null,
                true,
                true);

            Assert.Equal(HttpStatusCode.Redirect, transaction.Response.StatusCode);
            var redirectPath = transaction.Response.Headers.Location.OriginalString;
            var uri = new UriBuilder(redirectPath);
            Assert.Equal(this.Fixture.ServerHost, uri.Host);
            Assert.Equal("/oauth/user-consent", uri.Path);
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
            Assert.Single(queryDictionary);
            Assert.Equal(
                $"https://sso.mycompany.com/oauth/authorize?client_id=MyClientId&scope=wrongScope&response_type=code&redirect_uri=https://www.mycompany.com/faaastoauth/signin&state=CfDJ8Np1eFHOzoVJni6nVfHZpxxtPlOOOHr8csuyU7jfcKYoseFfn7kHq_e1yKbTVbDqvDoMNPIaoB0emAX8DhXQc7eOyIzHsYZYxwcsDLzcQIdrAMVre16lL2ni2c2F7s_6lY2p136sPyBtUi503YOndrnaKp6j3rlb",
                System.Web.HttpUtility.UrlDecode(queryDictionary["returnUrl"]));
        }

        [Fact]
        public async Task Test_invalidScope()
        {
            var server = this.Fixture.CreateServerApp(options => options.UserConsentPath = null, builder => builder.AddAuthorizationCodeFlow());
            var transaction = await this.QueryAsync(server, this.Fixture.Client.ClientId,
                "wrongScope",
                null,
                true,
                true);

            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.OAuth2Server.Resources.Msg_InvalidScope, transaction.ResponseText);
        }

        [Fact]
        public void Test_nominal()
        {
            //lock (this.Fixture)
            {
                this.Fixture.Code = null;
                var transaction = this.QueryAsync(this.Fixture.Client.ClientId,
                    TestClient.Scope,
                    null,
                    true,
                    true).Result;

                Assert.Equal(HttpStatusCode.Redirect, transaction.Response.StatusCode);
                var redirectPath = transaction.Response.Headers.Location.OriginalString;
                var uri = new UriBuilder(redirectPath);
                Assert.Equal(this.Fixture.ClientHost, uri.Host);
                Assert.Equal("/faaastoauth/signin", uri.Path);
                var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
                Assert.False(string.IsNullOrWhiteSpace(System.Web.HttpUtility.UrlDecode(queryDictionary["code"])));
                Assert.False(string.IsNullOrWhiteSpace(System.Web.HttpUtility.UrlDecode(queryDictionary["state"])));
                Assert.NotNull(this.Fixture.Code);
            }
        }

        [Fact]
        public void Test_nominal_withoutState()
        {
            //lock (this.Fixture)
            {
                this.Fixture.Code = null;
                var transaction = this.QueryAsync(this.Fixture.Client.ClientId,
                    TestClient.Scope,
                    null,
                    false,
                    true).Result;

                Assert.Equal(HttpStatusCode.Redirect, transaction.Response.StatusCode);
                var redirectPath = transaction.Response.Headers.Location.OriginalString;
                var uri = new UriBuilder(redirectPath);
                Assert.Equal(this.Fixture.ClientHost, uri.Host);
                Assert.Equal("/faaastoauth/signin", uri.Path);
                var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
                Assert.False(string.IsNullOrWhiteSpace(System.Web.HttpUtility.UrlDecode(queryDictionary["code"])));
                Assert.NotNull(System.Web.HttpUtility.UrlDecode(queryDictionary["state"]));
                Assert.NotNull(this.Fixture.Code);
            }
        }

        //[Fact]
        //public void Empty_authorizationEndpoint_throws_exception() => Assert.Throws<ArgumentException>(() => this.Fixture.CreateServerApp(options => options.AuthorizeEndpointPath = null, builder => builder.AddAuthorizationCodeFlow()));
    }
}
