﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Faaast.Authentication.OAuth2Server.Core
{
    public partial class OAuthServerMiddleware
    {
        private OAuthServerOptions Options { get; set; }

        private ILogger<OAuthServerMiddleware> Logger { get; set; }

        private JwtSecurityTokenHandler TokenHandler { get; set; }

        public OAuthServerMiddleware(RequestDelegate next, OAuthServerOptions options, ILoggerFactory loggerFactory)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
            Logger = loggerFactory.CreateLogger<OAuthServerMiddleware>();

            if (string.IsNullOrEmpty(options.Issuer))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.Issuer)), nameof(OAuthServerOptions.Issuer));
            }

            if (string.IsNullOrEmpty(options.LoginPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.Issuer)), nameof(OAuthServerOptions.Issuer));
            }

            if (string.IsNullOrEmpty(options.UserConsentPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.UserConsentPath)), nameof(OAuthServerOptions.UserConsentPath));
            }

            if (string.IsNullOrEmpty(options.TokenEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.TokenEndpointPath)), nameof(OAuthServerOptions.TokenEndpointPath));
            }

            if (string.IsNullOrEmpty(options.UserEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.UserEndpointPath)), nameof(OAuthServerOptions.UserEndpointPath));
            }

            if (string.IsNullOrEmpty(options.AuthorizeEndpointPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.AuthorizeEndpointPath)), nameof(OAuthServerOptions.AuthorizeEndpointPath));
            }

            options.TokenEndpointPath = options.TokenEndpointPath.ToLower();
            options.UserEndpointPath = options.UserEndpointPath.ToLower();
            options.AuthorizeEndpointPath = options.AuthorizeEndpointPath.ToLower();
            this.TokenHandler = new JwtSecurityTokenHandler();

            //TokenHandler.TokenLifetimeInMinutes = options.AccessTokenExpireTimeSpan.TotalMinutes;

        }

        private TokenValidationParameters BuildValidationParameters(ClientCredential client)
        {
            byte[] keybytes = Encoding.ASCII.GetBytes(client.ClientSecret);
            SecurityKey securityKey = new SymmetricSecurityKey(keybytes);
            //SigningCredentials signingCredentials =new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256Signature);

            return new TokenValidationParameters
            {
                RequireAudience = true,
                ValidateAudience = true,
                RequireExpirationTime = true,
                ValidAudience = client.Audience,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidIssuer = Options.Issuer,
                RequireSignedTokens = true,
                IssuerSigningKey = securityKey,
                ValidateIssuerSigningKey = true
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var provider = context.RequestServices.GetRequiredService<IOauthServerProvider>();
            ValidationContext validation = ValidationContext.Create(context);
            var result = await HandleEndpointRequestAsync(validation, provider);
            if (result?.IsValidated == false)
            {
                await Failed(context, result.Error);
            }
        }

        private async Task CreateJwt(ValidationContext validation, string accessToken, string refreshToken)
        {
            var options = new JsonWriterOptions
            {
                Indented = true
            };

            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, options))
                {
                    writer.WriteStartObject();
                    writer.WriteString("access_token", accessToken);
                    writer.WriteString("token_type", "bearer");
                    writer.WriteString("refresh_token", refreshToken);
                    writer.WriteNumber("expires_in", Options.AccessTokenExpireTimeSpan.TotalSeconds);
                    writer.WriteEndObject();
                }

                string json = Encoding.UTF8.GetString(stream.ToArray());
                await validation.HttpContext.Response.WriteAsync(json);
            }
        }

        protected Task<StageValidationContext> HandleEndpointRequestAsync(ValidationContext context, IOauthServerProvider provider)
        {
            StageValidationContext endpointValidation = new StageValidationContext(context);
            if (!Options.AllowInsecureHttp && !context.HttpContext.Request.IsHttps)
            {
                return endpointValidation.RejectAsync(ErrorCodes.invalid_request, Resources.Msg_Insecure);
            }

            string path = context.HttpContext.Request.Path.ToString().ToLower();

            if (path.Equals(Options.TokenEndpointPath))
                return HandleTokenEndPointRequestAsync(endpointValidation, provider);
            else if (path.Equals(Options.AuthorizeEndpointPath))
                return HandleAuthorizeEndPointRequestAsync(endpointValidation, provider);
            else if (path.Equals(Options.UserEndpointPath))
                return HandleUserEndPointRequestAsync(endpointValidation, provider);
            else
                return Task.FromResult<StageValidationContext>(null);
        }

        protected virtual Task<StageValidationContext> HandleTokenEndPointRequestAsync(StageValidationContext context, IOauthServerProvider provider) => context.GrantType?.ToLower() switch
        {
            Parameters.ClientCredentials => HandleClientCredentialsAsync(context, provider),
            Parameters.RefreshToken => HandleRefreshAsync(context, provider),
            Parameters.Password => HandlePasswordAsync(context, provider),
            Parameters.AuthorizationCode => HandleAuthorizationCodeAsync(context, provider),
            _ => context.RejectAsync(ErrorCodes.invalid_request, Resources.Msg_InvalidGrantType)
        };

        protected virtual async Task<StageValidationContext> HandleUserEndPointRequestAsync(StageValidationContext context, IOauthServerProvider provider)
        {
            StageValidationContext stage = new StageValidationContext(context);
            if (string.IsNullOrWhiteSpace(stage.AccessToken))
                return await stage.RejectAsync(ErrorCodes.access_denied, Resources.Msg_InvalidToken);

            JwtSecurityToken token = new JwtSecurityToken(stage.AccessToken);
            var payload = token.Payload;
            if(!payload.TryGetValue("clientId", out object clientIdObj))
                return await stage.RejectAsync(ErrorCodes.access_denied, Resources.Msg_InvalidToken);

            var client = await provider.ValidateCredentialsAsync(clientIdObj?.ToString());
            if(client == null)
                return await stage.RejectAsync(ErrorCodes.invalid_request, Resources.Msg_InvalidClient);

            var validation = BuildValidationParameters(client);
            var principal = this.TokenHandler.ValidateToken(context.AccessToken, validation, out var ValidatedToken);
            if (principal == null)
                return await stage.RejectAsync(ErrorCodes.access_denied, Resources.Msg_InvalidToken);

            //StageValidationContext validateClientContext = await ValidateClientCredentialsAsync(context, provider, false);
            //if (!validateClientContext.IsValidated)
            //    return validateClientContext;

            //BuildValidationParameters
            //if (!string.Equals(Options.Issuer, payload.Iss) ||
            //    payload.ValidTo < DateTime.UtcNow || 
            //    payload.
            //    )
            //    return stage.RejectAsync(ErrorCodes.access_denied, Resources.Msg_InvalidToken);
            //if()


            throw new NotImplementedException();
        }

        protected virtual Task<StageValidationContext> HandleRefreshAsync(ValidationContext context, IOauthServerProvider provider)
        {
            throw new NotImplementedException();
        }

        protected virtual Task<StageValidationContext> HandlePasswordAsync(ValidationContext context, IOauthServerProvider provider)
        {
            throw new NotImplementedException();
        }

        protected virtual Task<StageValidationContext> ValidateScopeAsync(StageValidationContext context, ClientCredential client, IOauthServerProvider provider)
        {
            StageValidationContext stageContext = new StageValidationContext(context);
            foreach (var contextScope in context.Scope)
            {
                bool found = false;
                foreach (var allowedScope in client.Scopes)
                {
                    if (allowedScope.Equals(contextScope, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return stageContext.RejectAsync(ErrorCodes.invalid_scope, Resources.Msg_InvalidScope);
            }

            return stageContext.ValidateAsync();
        }

        protected async Task Failed(HttpContext context, string error)
        {
            Logger.LogWarning(error);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            if (Options.DisplayErrors)
                await context.Response.WriteAsync(error);
            else
                await context.Response.WriteAsync("bad request");
        }

        protected virtual Task<StageValidationContext> RedirectAsync(StageValidationContext context, string relativeUri)
        {
            var request = context.HttpContext.Request;
            string currentUrl = BuildUri(request.Scheme, request.Host.Host, request.Host.Port, request.Path, request.QueryString.Value);
            string targetUrl = BuildUri(request.Scheme, request.Host.Host, request.Host.Port, relativeUri, $"?returnUrl={WebUtility.UrlEncode(currentUrl)}");
            context.HttpContext.Response.Redirect(targetUrl);
            return Task.FromResult<StageValidationContext>(null);
        }

        private string BuildUri(string scheme, string host, int? port, string path, string query)
        {
            int cleanPort = port ?? -1;
            UriBuilder sourceUrl = new UriBuilder(scheme, host, cleanPort, path, query);
            return sourceUrl.ToString();
        }

        private string GenerateAppSecretProof(string accessToken, ClientCredential client)
        {
            using (var algorithm = new HMACSHA256(Encoding.ASCII.GetBytes(client.ClientSecret)))
            {
                var hash = algorithm.ComputeHash(Encoding.ASCII.GetBytes(accessToken));
                var builder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }
    }
}
