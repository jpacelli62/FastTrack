using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Faaast.OAuth2Server.Abstraction;
using Faaast.OAuth2Server.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Faaast.OAuth2Server.Core.Flows
{
    public class UserEndpoint : OAuthMiddleware
    {
        public UserEndpoint(RequestDelegate next, OAuthServerOptions options, ILoggerFactory loggerFactory, ISystemClock clock) : base(next, options, loggerFactory, clock)
        {
        }

        protected override bool MatchEndpoint(RequestContext context) => this.Options.UserEndpointPath.Equals(context.HttpContext.Request.Path, StringComparison.OrdinalIgnoreCase);

        protected override bool ShouldHandle(RequestContext context) => HttpMethods.IsGet(context.HttpContext.Request.Method);

        protected override async Task<RequestResult<string>> HandleAsync(RequestContext context)
        {
            var result = new RequestResult<string>(context);
            var clientId = context.Require(Parameters.ClientId);
            var clientProvider = context.HttpContext.RequestServices.GetRequiredService<IOauthServerProvider>();
            var client = await clientProvider.GetClientAsync(clientId);
            if (client != null && client.IsAllowedFlow(nameof(UserEndpoint), context))
            {
                string token = context.Require(Parameters.AccessToken);
                var validationParams = this.BuildValidationParameters(client, context);
                var tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    ClaimsPrincipal principal = null;
                    principal = tokenHandler.ValidateToken(token, validationParams, out _);
                    var identity = principal.Identity as ClaimsIdentity;
                    identity.TryRemoveClaim(identity.FindFirst("nbf"));
                    identity.TryRemoveClaim(identity.FindFirst("exp"));
                    identity.TryRemoveClaim(identity.FindFirst("iss"));
                    identity.TryRemoveClaim(identity.FindFirst("iat"));
                    identity.TryRemoveClaim(identity.FindFirst("aud"));

                    var options = new JsonWriterOptions
                    {
                        Indented = true
                    };

                    using var stream = new MemoryStream();
                    using (var writer = new Utf8JsonWriter(stream, options))
                    {
                        writer.WriteStartObject();
                        foreach (var item in principal.Claims)
                        {
                            writer.WriteString(item.Type, item.Value);
                        }
                        writer.WriteEndObject();
                    }

                    return await result.Success(Encoding.UTF8.GetString(stream.ToArray()));

                }
                catch
                {
                    return await result.RejectAsync(Resources.Msg_InvalidToken);
                }
            }

            return await result.RejectAsync(Resources.Msg_InvalidClient);
        }

    }
}
