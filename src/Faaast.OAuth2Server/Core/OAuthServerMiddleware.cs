﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

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
            this.Logger = loggerFactory.CreateLogger<OAuthServerMiddleware>();

 #pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
           if (string.IsNullOrEmpty(options.Issuer))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.Issuer)), nameof(OAuthServerOptions.Issuer));
            }

            if (string.IsNullOrEmpty(options.LoginPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.LoginPath)), nameof(OAuthServerOptions.LoginPath));
            }

            if (string.IsNullOrEmpty(options.LogoutPath))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OAuthServerOptions.LogoutPath)), nameof(OAuthServerOptions.LogoutPath));
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
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 

        }

        private TokenValidationParameters BuildValidationParameters(Client client)
        {
            var keybytes = Encoding.ASCII.GetBytes(client.ClientSecret);
            SecurityKey securityKey = new SymmetricSecurityKey(keybytes);

            return new TokenValidationParameters
            {
                RequireAudience = true,
                ValidateAudience = true,
                RequireExpirationTime = true,
                ValidAudience = client.ClientId,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidIssuer = this.Options.Issuer,
                RequireSignedTokens = true,
                IssuerSigningKey = securityKey,
                ValidateIssuerSigningKey = true
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var provider = context.RequestServices.GetRequiredService<IOauthServerProvider>();
            var validation = ValidationContext.Create(context);
            var result = await this.HandleEndpointRequestAsync(validation, provider);
            if (result?.IsValidated == false)
            {
                await this.Failed(context, result.Error);
            }
        }

        private async Task CreateJwt(ValidationContext validation, AuthenticationTicket ticket, IOauthServerProvider provider)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(validation.Client.ClientSecret));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            Dictionary<string, object> claims = new();
            var excludeList = new[] { "iat", "exp", "iss", "nbf", "scope", "aud" };
            var groups = ticket.Principal.Claims.GroupBy(x => x.Type);
            foreach (var claimGroup in groups)
            {
                var add = true;
                foreach (var claimToExclude in excludeList)
                {
                    if (string.Equals(claimToExclude, claimGroup.Key))
                    {
                        add = false;
                        break;
                    }
                }

                if (add)
                {
                    if (claimGroup.Count() == 1)
                    {
                        var claim = claimGroup.First();
                        claims.Add(claim.Type, claim.Value);
                    }
                    else
                    {
                        var claim = claimGroup.First();
                        claims.Add(claim.Type, claimGroup.Select(x => x.Value).ToArray());
                    }
                }
            }

            claims.Add("scope", string.Join(" ", validation.Scope));

            var utcNow = DateTime.UtcNow;
            var expires = utcNow + this.Options.AccessTokenExpireTimeSpan;
            var jwtToken = this.TokenHandler.CreateJwtSecurityToken(new SecurityTokenDescriptor()
            {
                Audience = validation.Client.ClientId,
                Issuer = this.Options.Issuer,
                SigningCredentials = signingCredentials,
                IssuedAt = utcNow,
                Expires = expires,
                Claims = claims
            });

            var options = new JsonWriterOptions
            {
                Indented = true
            };

            var accessToken = this.TokenHandler.WriteToken(jwtToken);
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, options))
            {

                writer.WriteStartObject();
                writer.WriteString("access_token", accessToken);
                writer.WriteString("token_type", "bearer");
                writer.WriteNumber("expires_in", this.Options.AccessTokenExpireTimeSpan.TotalSeconds);

                if (validation.GrantType != Parameters.ClientCredentials)
                {
                    var refreshToken = CodeGenerator.GenerateRandomNumber(32);
                    writer.WriteString("refresh_token", refreshToken);

                    await provider.StoreAsync(new Token
                    {
                        AccessToken = accessToken,
                        AccessTokenExpiresUtc = expires,
                        RefreshToken = refreshToken,
                        RefreshTokenExpiresUtc = utcNow + this.Options.RefreshTokenExpireTimeSpan,
                        NameIdentifier = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    });

                }

                writer.WriteEndObject();
            }

            var json = Encoding.UTF8.GetString(stream.ToArray());
            await validation.HttpContext.Response.WriteAsync(json);
        }

        protected Task<StageValidationContext> HandleEndpointRequestAsync(ValidationContext context, IOauthServerProvider provider)
        {
            var endpointValidation = new StageValidationContext(context);
            if (!this.Options.AllowInsecureHttp && !context.HttpContext.Request.IsHttps)
            {
                return endpointValidation.RejectAsync(ErrorCodes.invalid_request, Resources.Msg_Insecure);
            }

            var path = context.HttpContext.Request.Path.ToString().ToLower();

            if (path.Equals(this.Options.TokenEndpointPath))
            {
                return this.HandleTokenEndPointRequestAsync(endpointValidation, provider);
            }
            else if (path.Equals(this.Options.AuthorizeEndpointPath))
            {
                return this.HandleAuthorizeEndPointRequestAsync(endpointValidation, provider);
            }
            else
            {
                return path.Equals(this.Options.LogoutPath)
                    ? this.HandleLogOutAsync(endpointValidation, provider)
                    : Task.FromResult<StageValidationContext>(null);
            }
        }

        protected virtual Task<StageValidationContext> HandleTokenEndPointRequestAsync(StageValidationContext context, IOauthServerProvider provider) => context.GrantType?.ToLower() switch
        {
            Parameters.ClientCredentials => this.HandleClientCredentialsAsync(context, provider),
            Parameters.RefreshToken => this.HandleRefreshAsync(context, provider),
            Parameters.Password => this.HandlePasswordAsync(context, provider),
            Parameters.AuthorizationCode => this.HandleAuthorizationCodeAsync(context, provider),
            _ => context.RejectAsync(ErrorCodes.invalid_request, Resources.Msg_InvalidGrantType)
        };

        protected virtual async Task<StageValidationContext> HandleRefreshAsync(ValidationContext context, IOauthServerProvider provider)
        {
            var validateClientContext = await this.ValidateClientCredentialsAsync(new StageValidationContext(context), provider, false);
            if (!validateClientContext.IsValidated)
            {
                return validateClientContext;
            }

            var validateRefreshTokenContext = new StageValidationContext(validateClientContext);
            if (string.IsNullOrWhiteSpace(validateRefreshTokenContext.RefreshToken))
            {
                return await validateRefreshTokenContext.RejectAsync(ErrorCodes.access_denied, Resources.Msg_InvalidToken);
            }

            var token = await provider.OnRefreshReceivedAsync(validateRefreshTokenContext.RefreshToken);
            if (token == null)
            {
                return await validateRefreshTokenContext.RejectAsync(ErrorCodes.access_denied, Resources.Msg_InvalidToken);
            }

            var validateAccessTokenContext = new StageValidationContext(validateRefreshTokenContext);
            var validationParams = this.BuildValidationParameters(validateAccessTokenContext.Client);
            validationParams.ValidateLifetime = false;
            var principal = this.TokenHandler.ValidateToken(token.AccessToken, validationParams, out _);
            if (principal == null)
            {
                return await validateAccessTokenContext.RejectAsync(ErrorCodes.access_denied, Resources.Msg_InvalidToken);
            }

            principal = await provider.OnRefreshPrincipaldAsync(principal);
            if (principal == null)
            {
                return await validateAccessTokenContext.RejectAsync(ErrorCodes.access_denied, Resources.Msg_InvalidToken);
            }

            var jwtToken = new JwtSecurityToken(token.AccessToken);
            validateAccessTokenContext.Scope = jwtToken.Payload["scope"].ToString().Split(' ');
            var authTicket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, "Default");

            await this.CreateJwt(validateAccessTokenContext, authTicket, provider);
            await validateAccessTokenContext.ValidateAsync();
            return validateAccessTokenContext;
        }

        protected virtual async Task<StageValidationContext> HandlePasswordAsync(ValidationContext context, IOauthServerProvider provider)
        {
            var validateClientContext = await this.ValidateClientCredentialsAsync(new StageValidationContext(context), provider, true);
            if (!validateClientContext.IsValidated)
            {
                return validateClientContext;
            }

            var validateClientUriContext = await this.ValidateRedirectUriAsync(validateClientContext, context.Client, provider);
            if (!validateClientUriContext.IsValidated)
            {
                return validateClientUriContext;
            }

            var validateScopesContext = await this.ValidateScopeAsync(validateClientUriContext, context.Client, provider);
            if (!validateScopesContext.IsValidated)
            {
                return validateScopesContext;
            }

            var validateAccountContext = new StageValidationContext(validateScopesContext);
            var principal = await provider.PasswordSigningAsync(context.UserName, context.Password);
            if (principal != null)
            {
                var authTicket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, "Default");

                await this.CreateJwt(validateAccountContext, authTicket, provider);
                await validateAccountContext.ValidateAsync();
                return validateAccountContext;
            }

            return await validateAccountContext.RejectAsync(ErrorCodes.access_denied, Resources.Msg_InvalidToken);
        }

        protected virtual Task<StageValidationContext> ValidateScopeAsync(StageValidationContext context, Client client, IOauthServerProvider provider)
        {
            var stageContext = new StageValidationContext(context);
            foreach (var contextScope in context.Scope)
            {
                var found = false;
                foreach (var allowedScope in client.Scopes)
                {
                    if (allowedScope.Equals(contextScope, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return stageContext.RejectAsync(ErrorCodes.invalid_scope, Resources.Msg_InvalidScope);
                }
            }

            return stageContext.ValidateAsync();
        }

        protected async Task Failed(HttpContext context, string error)
        {
            this.Logger.LogWarning(error);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            if (this.Options.DisplayErrors)
            {
                await context.Response.WriteAsync(error);
            }
            else
            {
                await context.Response.WriteAsync("bad request");
            }
        }

        protected virtual Task<StageValidationContext> RedirectAsync(StageValidationContext context, string relativeUri)
        {
            var request = context.HttpContext.Request;
            var currentUrl = BuildUri(request.Scheme, request.Host.Host, request.Host.Port, request.Path, request.QueryString.Value);
            var targetUrl = BuildUri(request.Scheme, request.Host.Host, request.Host.Port, relativeUri, $"?returnUrl={WebUtility.UrlEncode(currentUrl)}");
            context.HttpContext.Response.Redirect(targetUrl);
            return Task.FromResult<StageValidationContext>(null);
        }

        private static string BuildUri(string scheme, string host, int? port, string path, string query)
        {
            var cleanPort = port ?? -1;
            var sourceUrl = new UriBuilder(scheme, host, cleanPort, path, query);
            return sourceUrl.ToString();
        }

        protected virtual async Task<StageValidationContext> HandleLogOutAsync(StageValidationContext context, IOauthServerProvider provider)
        {
            var stage = new StageValidationContext(context);
            await stage.ValidateAsync();
            await context.HttpContext.SignOutAsync();
            context.HttpContext.Response.Redirect(stage.RedirectUri);
            return stage;
        }
    }
}
