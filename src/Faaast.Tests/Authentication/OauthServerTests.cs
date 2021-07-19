using Faaast.Authentication.OAuth2Server.Core;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using static Faaast.Tests.Authentication.AuthFixture;

namespace Faaast.Tests.Authentication
{
    public class OauthServerTests : IClassFixture<AuthFixture> 
    {
        public AuthFixture Fixture { get; set; }

        public TestServer Server { get; set; }

        public OauthServerTests(AuthFixture fixture)
        {
            this.Fixture = fixture;
            this.Server = Fixture.CreateServerApp(options => { });
        }

        private async Task<Transaction> SendClientCredentialsAsync(string clientId, string clientSecret, string audience)
        {
            return await Fixture.SendPostAsync(Server, Fixture.DefaultOptions.TokenEndpoint, new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId},
                { "client_secret", clientSecret },
                { "audience", audience }
            });
        }

        [Fact]
        public async Task Test_ClientCredentials_InvalidClient()
        {
            var transaction = await SendClientCredentialsAsync("wrongid", Fixture.DefaultOptions.ClientSecret, Audience);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.Authentication.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_ClientCredentials_InvalidPassword()
        {
            var transaction = await SendClientCredentialsAsync(Fixture.DefaultOptions.ClientId, "wrongpassword", Audience);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.Authentication.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_ClientCredentials_InvalidAudience()
        {
            var transaction = await SendClientCredentialsAsync(Fixture.DefaultOptions.ClientId, Fixture.DefaultOptions.ClientSecret, "wrongaudience");
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.Authentication.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_ClientCredentials_Valid()
        {
            var transaction = await SendClientCredentialsAsync(Fixture.DefaultOptions.ClientId, Fixture.DefaultOptions.ClientSecret, Audience);
            Assert.Equal(HttpStatusCode.OK, transaction.Response.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(transaction.ResponseText));
        }

        private string BuildAuthorizeUri(string clientId, string[] scopes, string redirectUri, string state)
        {
            var dic = new Dictionary<string, string>
            {
                {"client_id", clientId ?? Fixture.ClientId},
                {"scope", string.Join(' ', scopes ?? Fixture.DefaultOptions.Scope) },
                {"response_type", "code" },
                {"redirect_uri", redirectUri ?? "https://example.com/signin-oauth" },
                { "state", state ?? "CfDJ8Np1eFHOzoVJni6nVfHZpxxtPlOOOHr8csuyU7jfcKYoseFfn7kHq_e1yKbTVbDqvDoMNPIaoB0emAX8DhXQc7eOyIzHsYZYxwcsDLzcQIdrAMVre16lL2ni2c2F7s_6lY2p136sPyBtUi503YOndrnaKp6j3rlb"}
            };
            string uri = string.Concat(
                Fixture.DefaultOptions.AuthorizationEndpoint,
                "?",
                string.Join('&', dic.Select(x => string.Concat(x.Key, "=", System.Web.HttpUtility.UrlEncode(x.Value))).ToArray()));

            return uri;
        }

        [Fact]
        public async Task Test_Authorize_InvalidClient()
        {
            string uri = BuildAuthorizeUri("wrongclient", null, null, null);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var transaction = await Fixture.SendAsync(Server, request);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.Authentication.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_Authorize_InvalidRedirectUri()
        {
            string uri = BuildAuthorizeUri(null, null, "https://www.google.com", null);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var transaction = await Fixture.SendAsync(Server, request);
            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
            Assert.Equal(Faaast.Authentication.OAuth2Server.Resources.Msg_InvalidRedirectUri, transaction.ResponseText);
        }

        [Fact]
        public async Task Test_Authorize_RedirectToLogin()
        {
            string authorizeUri = BuildAuthorizeUri(null, null, null, null);
            var request = new HttpRequestMessage(HttpMethod.Get, authorizeUri);
            var transaction = await Fixture.SendAsync(Server, request);

            Assert.Equal(HttpStatusCode.Redirect, transaction.Response.StatusCode);
            string redirectPath = transaction.Response.Headers.Location.OriginalString;
            var uri = new UriBuilder(redirectPath);
            Assert.Equal(Fixture.ServerHost, uri.Host);
            Assert.Equal("/oauth/login", uri.Path);
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
            Assert.Single(queryDictionary);
            Assert.Equal(
                "https://sso.mycompany.com/oauth/authorize?client_id=myAppId&scope=identity&response_type=code&redirect_uri=https://example.com/signin-oauth&state=CfDJ8Np1eFHOzoVJni6nVfHZpxxtPlOOOHr8csuyU7jfcKYoseFfn7kHq_e1yKbTVbDqvDoMNPIaoB0emAX8DhXQc7eOyIzHsYZYxwcsDLzcQIdrAMVre16lL2ni2c2F7s_6lY2p136sPyBtUi503YOndrnaKp6j3rlb", 
                System.Web.HttpUtility.UrlDecode(queryDictionary["returnUrl"]));
        }

        [Fact]
        public async Task Test_Authorize_WithFakeAuthRequireUserConsent()
        {
            string authorizeUri = BuildAuthorizeUri(null, null, null, null);
            var request = new HttpRequestMessage(HttpMethod.Get, authorizeUri);
            request.Headers.Add("fakeLoggedUser", "1");
            request.Headers.Add("fakeRequireConsent", "1");
            var transaction = await Fixture.SendAsync(Server, request);

            Assert.Equal(HttpStatusCode.Redirect, transaction.Response.StatusCode);
            string redirectPath = transaction.Response.Headers.Location.OriginalString;
            var uri = new UriBuilder(redirectPath);
            Assert.Equal(Fixture.ServerHost, uri.Host);
            Assert.Equal("/oauth/user-consent", uri.Path);
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
            Assert.Single(queryDictionary);
            Assert.Equal(
                "https://sso.mycompany.com/oauth/authorize?client_id=myAppId&scope=identity&response_type=code&redirect_uri=https://example.com/signin-oauth&state=CfDJ8Np1eFHOzoVJni6nVfHZpxxtPlOOOHr8csuyU7jfcKYoseFfn7kHq_e1yKbTVbDqvDoMNPIaoB0emAX8DhXQc7eOyIzHsYZYxwcsDLzcQIdrAMVre16lL2ni2c2F7s_6lY2p136sPyBtUi503YOndrnaKp6j3rlb",
                System.Web.HttpUtility.UrlDecode(queryDictionary["returnUrl"]));
        }

        [Fact]
        public void TestCodeGenerator()
        {
            var code = CodeGenerator.GenerateRandomCode(32);
            var code2 = CodeGenerator.GenerateRandomCode(32);
            Assert.NotEqual(code, code2);
            Assert.Equal(32, code.Length);
            Assert.Equal(32, code2.Length);
        }

        [Fact]
        public async Task Test_Authorize_GiveAccessCode()
        {
            string authorizeUri = BuildAuthorizeUri(null, null, null, null);
            var request = new HttpRequestMessage(HttpMethod.Get, authorizeUri);
            request.Headers.Add("fakeLoggedUser", "1");
            var transaction = await Fixture.SendAsync(Server, request);

            Assert.Equal(HttpStatusCode.Redirect, transaction.Response.StatusCode);
            string redirectPath = transaction.Response.Headers.Location.OriginalString;
            var uri = new UriBuilder(redirectPath);
            Assert.Equal(Fixture.ClientHost, uri.Host);
            Assert.Equal("/signin-oauth", uri.Path);
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
            Assert.Equal(2, queryDictionary.Count);
            Assert.Equal("CfDJ8Np1eFHOzoVJni6nVfHZpxxtPlOOOHr8csuyU7jfcKYoseFfn7kHq_e1yKbTVbDqvDoMNPIaoB0emAX8DhXQc7eOyIzHsYZYxwcsDLzcQIdrAMVre16lL2ni2c2F7s_6lY2p136sPyBtUi503YOndrnaKp6j3rlb", queryDictionary["state"]);
            string code = queryDictionary["code"];
            Assert.False(string.IsNullOrWhiteSpace(code));
            Assert.Equal(32, code.Length);
        }
    }
}
