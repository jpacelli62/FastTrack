using Faaast.Authentication.OAuth2;
using Faaast.Authentication.OAuth2Server;
using Faaast.Authentication.OAuth2Server.Core;
using Faaast.OAuth2Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;
using Authorization = Faaast.Authentication.OAuth2Server.Authorization;

namespace Faaast.Tests.Authentication
{
    public class AuthFixture : IOauthServerProvider
    {
        public static readonly string Audience = "testApp";
        public readonly string ClientId = "myAppId";
        public readonly string ServerHost = "sso.mycompany.com";
        public readonly string ClientHost = "www.mycompany.com";

        public readonly string TokenEndpoint = "/oauth/token";
        public readonly string AuthorizeEndpoint = "/oauth/authorize";
        public readonly string UserEndpoint = "/oauth/user";

        private Dictionary<string, Authorization> Authorizations { get; set; }

        public FaaastOauthOptions DefaultOptions { get; set; }

        public void DefaultConfigure(FaaastOauthOptions options)
        {
            options.ClientId = ClientId;
            options.ClientSecret = "vcvbcbcvsdfsdfsdfdsfsdfsdfdsfdsfsdffsfdsfbvcb";
            options.OauthServerUri = $"https://{ServerHost}";
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }
        private Client Credential { get; set; }

        public AuthFixture()
        {
            Authorizations = new Dictionary<string, Authorization>();
            DefaultOptions = new FaaastOauthOptions();
            DefaultConfigure(DefaultOptions);

            Credential = new Client
            {
                ClientId = DefaultOptions.ClientId,
                ClientSecret = DefaultOptions.ClientSecret,
                Scopes = new[] { "identity" },
            };
        }

        private static void ConfigureClientApp(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.Use(async (context, next) =>
            {
                var request = context.Request;
                var response = context.Response;

                if (request.Path == new PathString("/"))
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                else if (request.Path == new PathString("/unauthorized"))
                {
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
                else if (request.Path == new PathString("/forbidden"))
                {
                    await context.ForbidAsync(FaaastOauthDefaults.AuthenticationScheme);
                }
                else if (request.Path == new PathString("/challenge"))
                {
                    await context.ChallengeAsync(FaaastOauthDefaults.AuthenticationScheme);
                }
                else
                {
                    await next();
                }
            });
        }
        private static void ConfigureClientServices(Action<FaaastOauthOptions> configureOptions, IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>()
               .AddDefaultTokenProviders();

            if (configureOptions != null)
            {
                services.AddAuthentication(FaaastOauthDefaults.AuthenticationScheme).AddFaaastOauth(configureOptions).AddCookie();
            }
            else
            {
                services.AddAuthentication(FaaastOauthDefaults.AuthenticationScheme).AddFaaastOauth();
            }
        }
        private void ConfigureServerApp(Action<OAuthServerOptions> configureOptions, IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var request = context.Request;

                if (request.Headers.ContainsKey("fakeLoggedUser"))
                {
                    List<Claim> claims = new List<Claim>();
                    context.User = new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
                }

                await next();
            });
            app.UseFaaastOAuthServer(options =>
            {
                options.DisplayErrors = true;
                options.AllowInsecureHttp = false;
                options.Issuer = ServerHost;
                if (configureOptions != null)
                    configureOptions(options);
            });
        }
        private void ConfigureServerServices(IServiceCollection services)
        {
            services.AddAuthentication();
            services.TryAddSingleton<IOauthServerProvider>(this);
        }

        public TestServer CreateApp(Action<IApplicationBuilder> configureApp, Action<IServiceCollection> configureOptions, Uri baseAddress = null)
        {
            var builder = new WebHostBuilder()
                .Configure(configureApp)
                .ConfigureServices(configureOptions);

            var server = new TestServer(builder)
            {
                BaseAddress = baseAddress
            };

            return server;
        }

        public TestServer CreateClientApp(Action<FaaastOauthOptions> clientConfig, Uri baseAddress = null)
        {
            return CreateApp(ConfigureClientApp, services => ConfigureClientServices(clientConfig, services), baseAddress);
        }

        public TestServer CreateServerApp(Action<OAuthServerOptions> serverConfig, Uri baseAddress = null)
        {
            return CreateApp(x => ConfigureServerApp(serverConfig, x), ConfigureServerServices, baseAddress);
        }

        public TestServer CreateMixedApp(Action<FaaastOauthOptions> clientConfig, Action<OAuthServerOptions> serverConfig, Uri baseAddress = null)
        {
            return CreateApp(
                app =>
                {
                    ConfigureClientApp(app);
                    ConfigureServerApp(serverConfig, app);
                },
                services =>
                {
                    ConfigureClientServices(clientConfig, services);
                    ConfigureServerServices(services);

                }, baseAddress);
        }

        public async Task<Transaction> SendPostAsync(TestServer server, string uri, Dictionary<string, string> values)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var content = new FormUrlEncodedContent(values.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)));
            request.Content = content;
            return await SendAsync(server, request);
        }
        public class Transaction
        {
            public HttpRequestMessage Request { get; set; }
            public HttpResponseMessage Response { get; set; }
            public string ResponseText { get; set; }
            public XElement ResponseElement { get; set; }
        }

        public async Task<Transaction> SendAsync(TestServer server, HttpRequestMessage request)
        {
            var transaction = new Transaction
            {
                Request = request,
                Response = await server.CreateClient().SendAsync(request),
            };

            transaction.ResponseText = await transaction.Response.Content.ReadAsStringAsync();

            if (transaction.Response.Content != null &&
                transaction.Response.Content.Headers.ContentType != null &&
                transaction.Response.Content.Headers.ContentType.MediaType == "text/xml")
            {
                transaction.ResponseElement = XElement.Parse(transaction.ResponseText);
            }

            return transaction;
        }


        private Token _token;
        public Task StoreAsync(Token token)
        {
            _token = token;
            return Task.CompletedTask;
        }

        public Task<Token> OnRefreshReceivedAsync(string refreshToken)
        {
            return Task.FromResult(_token);
        }

        public Task<ClaimsPrincipal> OnRefreshPrincipaldAsync(ClaimsPrincipal principal)
        {
            throw new NotImplementedException();
        }

        public Task<ClaimsPrincipal> PasswordSigningAsync(string login, string password)
        {
            throw new NotImplementedException();
        }

        public Task<Client> ValidateCredentialsAsync(string clientId)
        {
            if (Credential.ClientId == clientId)
                return Task.FromResult(Credential);

            return Task.FromResult<Client>(null);
        }

        public Task<Client> ValidateCredentialsAsync(string clientId, string clientSecret)
        {
            if (Credential.ClientId == clientId && Credential.ClientSecret == clientSecret)
                return Task.FromResult(Credential);

            return Task.FromResult<Client>(null);
        }

        public Task<bool> ValidateRedirectUriAsync(StageValidationContext context, Client client)
        {
            if ($"https://{ClientHost}/faaastoauth/signin".Equals(context.RedirectUri ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        public Task<bool> RequireUserConsentAsync(StageValidationContext context, Client client)
        {
            return Task.FromResult(context.HttpContext.Request.Headers.ContainsKey("fakeRequireConsent"));
        }

        public Task OnCreateAuthorizationCode(Authorization code)
        {
            Authorizations.Add(code.AuthorizationCode, code);
            return Task.CompletedTask;
        }

    public Task<Authorization> OnExchangeAuthorizationCode(string authorizationCode)
    {
        if (Authorizations.ContainsKey(authorizationCode))
            return Task.FromResult(Authorizations[authorizationCode]);

        return Task.FromResult<Authorization>(null);
    }
}
}
