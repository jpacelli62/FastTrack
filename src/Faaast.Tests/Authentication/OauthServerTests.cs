//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Faaast.Authentication.OAuth2Server.Core;
//using Microsoft.AspNetCore.TestHost;
//using Xunit;
//using static Faaast.Tests.Authentication.AuthFixture;

//namespace Faaast.Tests.Authentication
//{
//    public class OauthServerTests : IClassFixture<AuthFixture>
//    {
//        public AuthFixture Fixture { get; set; }

//        public TestServer Server { get; set; }

//        public OauthServerTests(AuthFixture fixture)
//        {
//            this.Fixture = fixture;
//            this.Server = this.Fixture.CreateServerApp(options => { });
//        }

       

//        //todo

//        //[Fact]
//        //public async Task Test_ClientCredentials_Valid()
//        //{
//        //    var transaction = await this.SendClientCredentialsAsync(this.Fixture.DefaultOptions.ClientId, this.Fixture.DefaultOptions.ClientSecret, Audience);
//        //    Assert.Equal(HttpStatusCode.OK, transaction.Response.StatusCode);
//        //    Assert.False(string.IsNullOrWhiteSpace(transaction.ResponseText));
//        //}

//        private string BuildAuthorizeUri(string clientId, string[] scopes, string redirectUri, string state)
//        {
//            var dic = new Dictionary<string, string>
//            {
//                {"client_id", clientId ?? this.Fixture.ClientId},
//                {"scope", string.Join(' ', scopes ?? this.Fixture.DefaultOptions.Scope) },
//                {"response_type", "code" },
//                {"redirect_uri", redirectUri ?? $"https://{this.Fixture.ClientHost}/faaastoauth/signin" },
//                { "state", state ?? "CfDJ8Np1eFHOzoVJni6nVfHZpxxtPlOOOHr8csuyU7jfcKYoseFfn7kHq_e1yKbTVbDqvDoMNPIaoB0emAX8DhXQc7eOyIzHsYZYxwcsDLzcQIdrAMVre16lL2ni2c2F7s_6lY2p136sPyBtUi503YOndrnaKp6j3rlb"}
//            };
//            var uri = string.Concat(
//                this.Fixture.DefaultOptions.AuthorizationEndpoint,
//                "?",
//                string.Join('&', dic.Select(x => string.Concat(x.Key, "=", System.Web.HttpUtility.UrlEncode(x.Value))).ToArray()));

//            return uri;
//        }

//        [Fact]
//        public async Task Test_Authorize_InvalidClient()
//        {
//            var uri = this.BuildAuthorizeUri("wrongclient", null, null, null);
//            var request = new HttpRequestMessage(HttpMethod.Get, uri);
//            var transaction = await SendAsync(this.Server, request);
//            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
//            Assert.Equal(Faaast.Authentication.OAuth2Server.Resources.Msg_InvalidClient, transaction.ResponseText);
//        }

//        [Fact]
//        public async Task Test_Authorize_InvalidRedirectUri()
//        {
//            var uri = this.BuildAuthorizeUri(null, null, "https://www.google.com", null);
//            var request = new HttpRequestMessage(HttpMethod.Get, uri);
//            var transaction = await SendAsync(this.Server, request);
//            Assert.Equal(HttpStatusCode.BadRequest, transaction.Response.StatusCode);
//            Assert.Equal(Faaast.Authentication.OAuth2Server.Resources.Msg_InvalidRedirectUri, transaction.ResponseText);
//        }

//        [Fact]
//        public async Task Test_Authorize_RedirectToLogin()
//        {
//            var authorizeUri = this.BuildAuthorizeUri(null, null, null, null);
//            var request = new HttpRequestMessage(HttpMethod.Get, authorizeUri);
//            var transaction = await SendAsync(this.Server, request);

//            Assert.Equal(HttpStatusCode.Redirect, transaction.Response.StatusCode);
//            var redirectPath = transaction.Response.Headers.Location.OriginalString;
//            var uri = new UriBuilder(redirectPath);
//            Assert.Equal(this.Fixture.ServerHost, uri.Host);
//            Assert.Equal("/oauth/login", uri.Path);
//            var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
//            Assert.Single(queryDictionary);
//            Assert.Equal(
//                $"https://sso.mycompany.com/oauth/authorize?client_id=myAppId&scope=identity&response_type=code&redirect_uri=https://{this.Fixture.ClientHost}/faaastoauth/signin&state=CfDJ8Np1eFHOzoVJni6nVfHZpxxtPlOOOHr8csuyU7jfcKYoseFfn7kHq_e1yKbTVbDqvDoMNPIaoB0emAX8DhXQc7eOyIzHsYZYxwcsDLzcQIdrAMVre16lL2ni2c2F7s_6lY2p136sPyBtUi503YOndrnaKp6j3rlb",
//                System.Web.HttpUtility.UrlDecode(queryDictionary["returnUrl"]));
//        }

//        [Fact]
//        public async Task Test_Authorize_WithFakeAuthRequireUserConsent()
//        {
//            var authorizeUri = this.BuildAuthorizeUri(null, null, null, null);
//            var request = new HttpRequestMessage(HttpMethod.Get, authorizeUri);
//            request.Headers.Add("fakeLoggedUser", "1");
//            request.Headers.Add("fakeRequireConsent", "1");
//            var transaction = await SendAsync(this.Server, request);

//            Assert.Equal(HttpStatusCode.Redirect, transaction.Response.StatusCode);
//            var redirectPath = transaction.Response.Headers.Location.OriginalString;
//            var uri = new UriBuilder(redirectPath);
//            Assert.Equal(this.Fixture.ServerHost, uri.Host);
//            Assert.Equal("/oauth/user-consent", uri.Path);
//            var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
//            Assert.Single(queryDictionary);
//            Assert.Equal(
//                $"https://sso.mycompany.com/oauth/authorize?client_id=myAppId&scope=identity&response_type=code&redirect_uri=https://{this.Fixture.ClientHost}/faaastoauth/signin&state=CfDJ8Np1eFHOzoVJni6nVfHZpxxtPlOOOHr8csuyU7jfcKYoseFfn7kHq_e1yKbTVbDqvDoMNPIaoB0emAX8DhXQc7eOyIzHsYZYxwcsDLzcQIdrAMVre16lL2ni2c2F7s_6lY2p136sPyBtUi503YOndrnaKp6j3rlb",
//                System.Web.HttpUtility.UrlDecode(queryDictionary["returnUrl"]));
//        }

//        [Fact]
//        public void TestCodeGenerator()
//        {
//            var code = CodeGenerator.GenerateRandomCode(32);
//            var code2 = CodeGenerator.GenerateRandomCode(32);
//            Assert.NotEqual(code, code2);
//            Assert.Equal(32, code.Length);
//            Assert.Equal(32, code2.Length);
//        }

//        [Fact]
//        public async Task Test_Authorize_GiveAccessCode()
//        {
//            var authorizeUri = this.BuildAuthorizeUri(null, null, null, null);
//            var request = new HttpRequestMessage(HttpMethod.Get, authorizeUri);
//            request.Headers.Add("fakeLoggedUser", "1");
//            var transaction = await SendAsync(this.Server, request);

//            Assert.Equal(HttpStatusCode.Redirect, transaction.Response.StatusCode);
//            var redirectPath = transaction.Response.Headers.Location.OriginalString;
//            var uri = new UriBuilder(redirectPath);
//            Assert.Equal(this.Fixture.ClientHost, uri.Host);
//            Assert.Equal("/faaastoauth/signin", uri.Path);
//            var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
//            Assert.Equal(2, queryDictionary.Count);
//            Assert.Equal("CfDJ8Np1eFHOzoVJni6nVfHZpxxtPlOOOHr8csuyU7jfcKYoseFfn7kHq_e1yKbTVbDqvDoMNPIaoB0emAX8DhXQc7eOyIzHsYZYxwcsDLzcQIdrAMVre16lL2ni2c2F7s_6lY2p136sPyBtUi503YOndrnaKp6j3rlb", queryDictionary["state"]);
//            var code = queryDictionary["code"];
//            Assert.False(string.IsNullOrWhiteSpace(code));
//            Assert.Equal(32, code.Length);
//        }
//    }
//}
