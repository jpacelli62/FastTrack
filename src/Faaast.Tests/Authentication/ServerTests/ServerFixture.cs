using System;
using System.Linq;
using System.Threading.Tasks;
using Faaast.OAuth2Server;
using Faaast.OAuth2Server.Abstraction;
using Faaast.OAuth2Server.Configuration;
using Faaast.Tests.Authentication.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;

namespace Faaast.Tests.Authentication.ServerTests
{
    public class ServerFixture : IOauthServerProvider
    {
        public readonly string ServerHost = "sso.mycompany.com";
        public readonly string ClientHost = "www.mycompany.com";

        public string TokenEndpoint => $"https://{ServerHost}/oauth/token";
        public string AuthorizeEndpoint => $"https://{ServerHost}/oauth/authorize";
        public string UserEndpoint => $"https://{ServerHost}/oauth/user";

        public TestClient Client { get; set; }

        public AuthorizationCode Code { get; set; }

        public Token Token { get; set; }

        public ISystemClock Clock { get; set; }

        public ServerFixture()
        {
            this.Client = new TestClient();
            var clockMock = new Moq.Mock<ISystemClock>();
            var now = new DateTimeOffset(DateTime.UtcNow);
            clockMock.Setup(x => x.UtcNow).Returns(now);
            this.Clock = clockMock.Object;
        }

        public TestServer CreateServerApp(Action<OAuthServerOptions> serverConfig, Action<IOauthBuilder> builder, Uri baseAddress = null) => ServerUtils.CreateServer(app =>
                {
                    app.Use(async (context, next) =>
                    {
                        var request = context.Request;

                        if (request.Headers.ContainsKey("fakeLoggedUser"))
                        {
                            var claims = new List<Claim>();
                            context.User = new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
                        }

                        await next();
                    });
                    builder(app.UseFaaastOAuthServer(options =>
                    {
                        options.AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(10);
                        options.RefreshTokenExpireTimeSpan = TimeSpan.FromMinutes(100);
                        options.DisplayDetailedErrors = true;
                        options.AllowInsecureHttp = true;
                        options.Issuer = ServerHost;
                        serverConfig?.Invoke(options);
                    }));
                },
                services =>
                {
                    services.TryAddSingleton<ISystemClock>(this.Clock);
                    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => options.LoginPath = "/login");
                    services.TryAddSingleton<IOauthServerProvider>(this);
                    services.AddFaaastOAuthServer();
                },
                baseAddress);

        public Task<IClient> GetClientAsync(string clientId) => clientId switch
        {
            "MyClientId" or "disallowFlow" => Task.FromResult<IClient>(this.Client),
            "throw" => throw new ArgumentException("test error message"),
            _ => Task.FromResult<IClient>(null),
        };

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
    }
}
