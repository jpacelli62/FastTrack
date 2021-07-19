using Faaast.Authentication.OAuth2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Faaast.Tests.Authentication
{
    public class OAuth2ClientTests : IClassFixture<AuthFixture>
    {
        public AuthFixture Fixture { get; set; }

        public OAuth2ClientTests(AuthFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Fact]
        public async Task VerifySchemeDefaults()
        {
            var services = new ServiceCollection();
            services.AddAuthentication(FaaastOauthDefaults.AuthenticationScheme).AddFaaastOauth(Fixture.DefaultConfigure); 
            var sp = services.BuildServiceProvider();
            var schemeProvider = sp.GetRequiredService<IAuthenticationSchemeProvider>();
            var scheme = await schemeProvider.GetSchemeAsync(FaaastOauthDefaults.AuthenticationScheme);
            Assert.NotNull(scheme);
            Assert.Equal(nameof(FaaastOauthHandler), scheme.HandlerType.Name);
            Assert.NotNull(scheme.DisplayName);
        }

        [Fact]
        public async Task NormalRequestPassesThrough()
        {
            var server = Fixture.CreateClientApp(Fixture.DefaultConfigure);
            var response = await server.CreateClient().GetAsync("https://example.com/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedPathReturnsUnauthorized()
        {
            var server = Fixture.CreateClientApp(Fixture.DefaultConfigure);
            var response = await server.CreateClient().GetAsync("https://example.com/unauthorized");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ForbiddenPathReturnsForbiddenStatus()
        {
            var server = Fixture.CreateClientApp(Fixture.DefaultConfigure);
            var response = await server.CreateClient().GetAsync("https://example.com/forbidden");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Account/AccessDenied?ReturnUrl=%2Fforbidden", response.Headers.Location.PathAndQuery);
        }

        [Fact]
        public async Task ChallengePathRedirectToAuthorizationServer()
        {
            var server = Fixture.CreateClientApp(Fixture.DefaultConfigure);
            var response = await server.CreateClient().GetAsync("https://example.com/challenge");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            string redirectPath = response.Headers.Location.OriginalString;
            var uri = new UriBuilder(redirectPath);
            Assert.Equal(Fixture.ServerHost, uri.Host);
            Assert.Equal(Fixture.AuthorizeEndpoint, uri.Path);
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
            Assert.Equal(Fixture.ClientId, queryDictionary["client_id"]);
            Assert.Equal("identity", queryDictionary["scope"]);
            Assert.Equal("https://example.com/signin-oauth", System.Web.HttpUtility.UrlDecode(queryDictionary["redirect_uri"]));
            Assert.False(string.IsNullOrWhiteSpace(queryDictionary["state"]));
            Assert.Equal(5, queryDictionary.Count);
        }
    }
}
