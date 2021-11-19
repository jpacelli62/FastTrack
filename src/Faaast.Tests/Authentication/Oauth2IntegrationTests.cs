using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Faaast.Authentication.OAuth2;
using Xunit;

namespace Faaast.Tests.Authentication
{
    public class Oauth2IntegrationTests : IClassFixture<AuthFixture>
    {
        public AuthFixture Fixture { get; set; }

        public Oauth2IntegrationTests(AuthFixture fixture) => this.Fixture = fixture;

        [Fact]
        public async Task OAuthIntegrationTest()
        {
            FaaastOauthOptions option = null;
            var server = this.Fixture.CreateMixedApp(options =>// Client & server on same host
            {
                option = options;
                options.ClientId = this.Fixture.DefaultOptions.ClientId;
                options.ClientSecret = this.Fixture.DefaultOptions.ClientSecret;
                options.OauthServerUri = this.Fixture.DefaultOptions.OauthServerUri;
                options.SignInScheme = this.Fixture.DefaultOptions.SignInScheme;
            }, options => { });

            // Step 1 : challenge login on client app redirects to the server authorize endpoint
            var client = server.CreateClient();
            var clientResponse = await client.GetAsync($"https://{this.Fixture.ClientHost}/challenge");
            Assert.Equal(HttpStatusCode.Redirect, clientResponse.StatusCode);
            var correlationCookie = clientResponse.Headers.GetValues("Set-Cookie").First();
            var redirectPath = clientResponse.Headers.Location.OriginalString;
            var uri = new UriBuilder(redirectPath);
            Assert.Equal(this.Fixture.AuthorizeEndpoint, uri.Path);

            //Step 2 : Call to the server with a fake logged user an no consent required result to authorization code
            var request = new HttpRequestMessage(HttpMethod.Get, redirectPath);
            request.Headers.Add("fakeLoggedUser", "1");
            var serverAuthorizationToken = await AuthFixture.SendAsync(server, request);
            var callbackPath1 = serverAuthorizationToken.Response.Headers.Location.OriginalString;
            var uri2 = new UriBuilder(callbackPath1);
            Assert.Equal(this.Fixture.DefaultOptions.CallbackPath, uri2.Path);
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri2.Query);
            Assert.Contains("code", queryDictionary.AllKeys);
            Assert.Contains("state", queryDictionary.AllKeys);

            //Step 3 : Give authorizationToken to client handler to exchange it
            option.Backchannel = client;
            client.DefaultRequestHeaders.Add("Cookie", correlationCookie);
            var response2 = await client.GetAsync(callbackPath1);
            Assert.Equal(HttpStatusCode.Redirect, response2.StatusCode);

            //https://sso.mycompany.com/oauth/user?access_token=accessToken&appsecret_proof=f2b326ab67970b3863a3290040e7ffd437b7fea3c3f90d060fb6c167e6b20fed
        }

        //
    }
}
