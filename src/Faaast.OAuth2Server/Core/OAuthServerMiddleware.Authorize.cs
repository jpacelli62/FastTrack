using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Faaast.Authentication.OAuth2Server.Core
{
    public partial class OAuthServerMiddleware
    {
        protected virtual async Task<StageValidationContext> HandleAuthorizeEndPointRequestAsync(StageValidationContext context, IOauthServerProvider provider)
        {
            var validateClientContext = await this.ValidateClientCredentialsAsync(context, provider, false);
            if (!validateClientContext.IsValidated)
            {
                return validateClientContext;
            }

            var validateClientUriContext = await this.ValidateRedirectUriAsync(validateClientContext, context.Client, provider);
            if (!validateClientUriContext.IsValidated)
            {
                return validateClientUriContext;
            }

            var validateScopesContext = await this.ValidateScopeAsync(context, context.Client, provider);
            if (!validateScopesContext.IsValidated)
            {
                return validateScopesContext;
            }

            if (!context.HttpContext.User?.Identity.IsAuthenticated ?? false)
            {
                return await this.RedirectAsync(context, this.Options.LoginPath);
            }

            var validateConsentContext = new StageValidationContext(context);
            var requireConsent = await provider.RequireUserConsentAsync(validateConsentContext, context.Client);
            if (requireConsent)
            {
                return await this.RedirectAsync(context, this.Options.UserConsentPath);
            }

            var code = CodeGenerator.GenerateRandomCode(32);
            var uri = new Uri(context.RedirectUri);
            var query = string.Format("?code={0}&state={1}", code, context.State ?? string.Empty);
            var sourceUrl = new UriBuilder(uri.Scheme, uri.Host, uri.Port, uri.LocalPath, query);

            var properties = new AuthenticationProperties
            {
                RedirectUri = context.RedirectUri,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow + TimeSpan.FromMinutes(5)
            };
            properties.SetString("Scope", string.Join(" ", context.Scope));
            await provider.OnCreateAuthorizationCode(new Authorization
            {
                AuthorizationCode = code,
                AuthenticationTicket = new AuthenticationTicket(context.HttpContext.User, properties, "Default")
            });

            context.HttpContext.Response.Redirect(sourceUrl.ToString());
            return null;
        }

        protected virtual async Task<StageValidationContext> ValidateRedirectUriAsync(StageValidationContext context, Client client, IOauthServerProvider provider)
        {
            var redirectUriContext = new StageValidationContext(context);
            var isValidated = await provider.ValidateRedirectUriAsync(redirectUriContext, client);
            if (!isValidated)
            {
                await redirectUriContext.RejectAsync(ErrorCodes.invalid_request, Resources.Msg_InvalidRedirectUri);
            }
            else
            {
                await redirectUriContext.ValidateAsync();
            }

            return redirectUriContext;
        }

        protected virtual async Task<StageValidationContext> HandleAuthorizationCodeAsync(StageValidationContext context, IOauthServerProvider provider)
        {
            var stageContext = new StageValidationContext(context);

            var validateClientContext = await this.ValidateClientCredentialsAsync(context, provider, true);
            if (!validateClientContext.IsValidated)
            {
                return validateClientContext;
            }

            var ValidateAuthorizationContext = new StageValidationContext(context);
            var authorization = await provider.OnExchangeAuthorizationCode(context.Code);
            if (authorization == null)
            {
                return await ValidateAuthorizationContext.RejectAsync(ErrorCodes.access_denied, Resources.Msg_InvalidCode);
            }

            var validateRedirectUriContext = await this.ValidateRedirectUriAsync(validateClientContext, context.Client, provider);
            if (validateRedirectUriContext.IsValidated && !string.Equals(authorization.AuthenticationTicket.Properties.RedirectUri, context.RedirectUri, StringComparison.OrdinalIgnoreCase))
            {
                return await validateRedirectUriContext.RejectAsync(ErrorCodes.invalid_request, Resources.Msg_InvalidRedirectUri);
            }

            if (!validateRedirectUriContext.IsValidated)
            {
                return validateRedirectUriContext;
            }

            context.Scope = authorization.AuthenticationTicket.Properties.GetString("Scope").Split(" ".ToCharArray());
            var validateScopesContext = await this.ValidateScopeAsync(context, context.Client, provider);
            if (!validateScopesContext.IsValidated)
            {
                return validateScopesContext;
            }

            await this.CreateJwt(context, authorization.AuthenticationTicket, provider);

            await stageContext.ValidateAsync();
            return stageContext;
        }
    }
}
