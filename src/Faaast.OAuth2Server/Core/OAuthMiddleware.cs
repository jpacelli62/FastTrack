using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Abstraction;
using Faaast.OAuth2Server.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Faaast.OAuth2Server.Core
{
    public abstract class OAuthMiddleware
    {
        protected OAuthServerOptions Options { get; set; }

        protected ILogger<OAuthMiddleware> Logger { get; set; }

        protected RequestDelegate Next { get; set; }

        protected JwtSecurityTokenHandler TokenHandler { get; set; }

        protected ISystemClock Clock { get; set; }

        protected OAuthMiddleware(RequestDelegate next, OAuthServerOptions options, ILoggerFactory loggerFactory, ISystemClock clock)
        {
            this.Options = options;
            this.Logger = loggerFactory.CreateLogger<OAuthMiddleware>();
            this.Next = next;
            this.TokenHandler = new JwtSecurityTokenHandler();
            this.Clock = clock;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = new RequestContext(context);
            if (this.MatchEndpoint(request) && this.ShouldHandle(request))
            {
                if (!this.Options.AllowInsecureHttp && !context.Request.IsHttps)
                {
                    await this.Failed(context, Resources.Msg_Insecure);
                    return;
                }

                await this.InvokeAsync(request);
            }
            else
            {
                await this.Next(context);
            }
        }

        private async Task InvokeAsync(RequestContext request)
        {
            try
            {
                var result = await this.HandleAsync(request);
                if (result != null)
                {
                    if (result.IsValidated)
                    {
                        if (!string.IsNullOrEmpty(result.Result))
                        {
                            await request.HttpContext.Response.WriteAsync(result.Result);
                        }
                    }
                    else
                    {
                        await this.Failed(result);
                    }
                }
            }
            catch (RequestException req)
            {
                await this.Failed(request.HttpContext, string.Format(Resources.Msg_RequestException, req.ParameterName, req.ExpectedMethod));
            }
            catch (Exception ex)
            {
                await this.Fatal(request.HttpContext, ex);
            }
        }

        protected abstract bool MatchEndpoint(RequestContext context);

        protected abstract bool ShouldHandle(RequestContext context);

        protected abstract Task<RequestResult<string>> HandleAsync(RequestContext context);

        protected Task Failed(RequestResult<string> result) => this.Failed(result.Context.HttpContext, result.Error, result.StatusCode);

        protected async Task Failed(HttpContext context, string error, int statusCode = StatusCodes.Status400BadRequest)
        {
            this.Logger.LogWarning(error);
            context.Response.StatusCode = statusCode;
            if (this.Options.DisplayDetailedErrors)
            {
                await context.Response.WriteAsync(error);
            }
            else
            {
                await context.Response.WriteAsync("bad request");
            }
        }

        protected async Task Fatal(HttpContext context, Exception ex)
        {
            this.Logger.LogError(ex, ex.Message);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            if (this.Options.DisplayDetailedErrors)
            {
                await context.Response.WriteAsync($"{ex.GetType().Name} : {ex.Message}");
            }
            else
            {
                await context.Response.WriteAsync("internal server error");
            }
        }

        protected string CreateJwtToken(RequestContext context, IClient client, string audience, AuthenticationTicket ticket)
        {
            var utcNow = this.Clock.UtcNow;
            var expires = utcNow + this.Options.AccessTokenExpireTimeSpan;
            var jwtToken = this.TokenHandler.CreateJwtSecurityToken(new SecurityTokenDescriptor()
            {
                Audience = audience,
                Issuer = this.Options.Issuer,
                SigningCredentials = client.GetSigninCredentials(context),
                IssuedAt = utcNow.UtcDateTime,
                Expires = expires.UtcDateTime,
                Subject = ticket.Principal.Identity as ClaimsIdentity
            });

            return this.TokenHandler.WriteToken(jwtToken);
        }

        protected string CreateJwtResponse(Token token)
        {
            var options = new JsonWriterOptions
            {
                Indented = true
            };

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, options))
            {
                writer.WriteStartObject();
                writer.WriteString("access_token", token.AccessToken);
                writer.WriteString("token_type", "bearer");
                writer.WriteNumber("expires_in", this.Options.AccessTokenExpireTimeSpan.TotalSeconds);

                if (!string.IsNullOrEmpty(token.RefreshToken))
                {
                    writer.WriteString("refresh_token", token.RefreshToken);
                }

                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        protected virtual Task<RequestResult<string>> RedirectAsync(RequestContext context, string relativeUri)
        {
            var request = context.HttpContext.Request;
            var currentUrl = BuildUri(request.Scheme, request.Host.Host, request.Host.Port, request.Path, request.QueryString.Value);
            var targetUrl = BuildUri(request.Scheme, request.Host.Host, request.Host.Port, relativeUri, $"?returnUrl={WebUtility.UrlEncode(currentUrl)}");
            context.HttpContext.Response.Redirect(targetUrl);
            var result = new RequestResult<string>(context);
            return result.Success(null);
        }

        internal static string BuildUri(string scheme, string host, int? port, string path, string query)
        {
            var cleanPort = port ?? -1;
            var sourceUrl = new UriBuilder(scheme, host, cleanPort, path, query);
            return sourceUrl.ToString();
        }
    }
}
