using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Faaast.OAuth2Server;
using Faaast.OAuth2Server.Abstraction;
using Faaast.OAuth2Server.Configuration;
using Faaast.OAuth2Server.Core;
using Faaast.Tests.Authentication.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Faaast.Tests.Authentication.ServerTests
{
    public class ServerFixture : CustomTestServer, IOauthServerProvider, IAuthorizationCodeProvider, IClientCredentialsProvider, IRefreshTokenProvider, IResourceOwnerPasswordProvider
    {
        public readonly string ClientHost = "www.mycompany.com";

        public string TokenEndpoint { get => $"https://{Options.Issuer}{Options.TokenEndpointPath}"; }

        public string AuthorizeEndpoint { get => $"https://{Options.Issuer}{Options.AuthorizeEndpointPath}"; }

        public TestClient Client { get; set; }

        public AuthorizationCode Code { get; set; }

        public Token Token { get; set; }

        public OAuthServerOptions Options { get; set; }

        public ServerFixture()
        {
            this.Client = new TestClient();
        }

        public CustomTestServer CreateServer(Action<IOauthBuilder> oauth, Action<OAuthServerOptions> options = null)
        {
            CustomTestServer server = new CustomTestServer()
            {
                ConfigureServices = services =>
                {
                    services.TryAddSingleton<IAuthorizationCodeProvider>(this);
                    services.TryAddSingleton<IClientCredentialsProvider>(this);
                    services.TryAddSingleton<IRefreshTokenProvider>(this);
                    services.TryAddSingleton<IResourceOwnerPasswordProvider>(this);
                    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => options.LoginPath = "/login");
                    services.Configure<OAuthServerOptions>(Configuration.GetSection("OAuthServerOptions"));
                    services.Configure<TestClient>(Configuration.GetSection("TestClient"));
                    services.AddFaaastOAuthServer<ServerFixture>(this, options);
                },
                Configure = app =>
                {
                    this.Options = app.ApplicationServices.GetRequiredService<IOptions<OAuthServerOptions>>().Value;
                    this.Client = app.ApplicationServices.GetRequiredService<IOptions<TestClient>>().Value;
                    app.Use(async (context, next) =>
                    {
                        var request = context.Request;

                        if (request.Headers.ContainsKey("fakeLoggedUser"))
                        {
                            var claims = new List<Claim>();
                            claims.Add(new Claim(ClaimTypes.Name, "John Doe"));
                            context.User = new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
                        }

                        await next();
                    });
                    oauth(app.UseFaaastOAuthServer());
                }
            };

            server.Start();
            return server;
        }


        public Task<IClient> GetClientAsync(string clientId) => clientId == this.Client.ClientId ? Task.FromResult<IClient>(this.Client) : Task.FromResult<IClient>(null);

        public Task StoreAsync(Token token)
        {
            this.Token = token;
            return Task.CompletedTask;
        }

        public Task OnCreateAuthorizationCodeAsync(AuthorizationCode code)
        {
            this.Code = code;
            return Task.CompletedTask;
        }

        public Task<AuthorizationCode> OnExchangeAuthorizationCodeAsync(string code) => Task.FromResult<AuthorizationCode>(this.Code?.Code == code ? this.Code : null);

        public Task<RequestResult<ClaimsIdentity>> PasswordSignInAsync(string username, string password, IClient client, RequestContext context)
        {
            if (context.HttpContext.Request.Headers.TryGetValue(nameof(PasswordSignInAsync), out var behaviour))
            {
                if (behaviour.Equals("logged"))
                {

                    List<Claim> claims = new();
                    claims.Add(new Claim(ClaimTypes.Name, "John Doe"));
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, "123"));
                    RequestResult<ClaimsIdentity> result = new(context);
                    return result.Success(new ClaimsIdentity(claims));
                }
                else if (behaviour.Equals("locked"))
                {
                    RequestResult<ClaimsIdentity> result = new(context);
                    return result.RejectAsync("Locked account");
                }
                else if (behaviour.Equals("crash"))
                {
                    throw new ArgumentException("test error message");
                }
            }

            return Task.FromResult<RequestResult<ClaimsIdentity>>(null);
        }

        public Task<Token> OnRefreshTokenReceivedAsync(string refreshToken, RequestContext context) => string.Compare(refreshToken, this.Token?.RefreshToken) == 0 ? Task.FromResult<Token>(this.Token) : Task.FromResult<Token>(null);

        public Task<ClaimsIdentity> CreateClientIdentityAsync(RequestContext context, IClient client)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "-1"),
                new Claim(ClaimTypes.Role, client.ClientId)
            };
            return Task.FromResult(new ClaimsIdentity(claims, "identity.application"));
        }

        public JwtPayload Read(string response, bool expectedRefresh)
        {
            var jsonResult = JObject.Parse(response);
            var access_token = jsonResult.SelectToken("access_token").Value<string>();

            Assert.NotNull(access_token);
            if (expectedRefresh)
            {
                var refresh_token = jsonResult.SelectToken("refresh_token").Value<string>();
                Assert.NotNull(refresh_token);
            }

            JwtSecurityToken token = new JwtSecurityToken(access_token);
            var payload = token.Payload;
            Assert.Equal(this.Options.Issuer, payload.Iss);
        
            return payload;
        }
    }
}
