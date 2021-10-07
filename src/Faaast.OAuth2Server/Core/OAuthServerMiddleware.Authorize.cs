using Microsoft.AspNetCore.Authentication;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Faaast.Authentication.OAuth2Server.Core
{
    public partial class OAuthServerMiddleware
    {
        protected virtual async Task<StageValidationContext> HandleAuthorizeEndPointRequestAsync(StageValidationContext context, IOauthServerProvider provider)
        {
            StageValidationContext validateClientContext = await ValidateClientCredentialsAsync(context, provider, false);
            if (!validateClientContext.IsValidated)
                return validateClientContext;

            StageValidationContext validateClientUriContext = await ValidateRedirectUriAsync(validateClientContext, context.Client, provider);
            if (!validateClientUriContext.IsValidated)
                return validateClientUriContext;

            StageValidationContext validateScopesContext = await ValidateScopeAsync(context, context.Client, provider);
            if (!validateScopesContext.IsValidated)
                return validateScopesContext;


            if (!context.HttpContext.User?.Identity.IsAuthenticated ?? false)
            {
                return await RedirectAsync(context, Options.LoginPath);
            }

            StageValidationContext validateConsentContext = new StageValidationContext(context);
            bool requireConsent = await provider.RequireUserConsentAsync(validateConsentContext, context.Client);
            if (requireConsent)
            {
                return await RedirectAsync(context, Options.UserConsentPath);
            }

            string code = CodeGenerator.GenerateRandomCode(32);
            var uri = new Uri(context.RedirectUri);
            string query = string.Format("?code={0}&state={1}", code, context.State ?? string.Empty);
            UriBuilder sourceUrl = new UriBuilder(uri.Scheme, uri.Host, uri.Port, uri.LocalPath, query);

            AuthenticationProperties properties = new AuthenticationProperties();
            properties.RedirectUri = context.RedirectUri;
            properties.IssuedUtc = DateTime.UtcNow;
            properties.ExpiresUtc = DateTime.UtcNow + TimeSpan.FromMinutes(5);
            properties.SetString("Scope", string.Join(" ", context.Scope));
            await provider.OnCreateAuthorizationCode(new Authorization
            {
                AuthorizationCode = code,
                AuthenticationTicket = new AuthenticationTicket(context.HttpContext.User, properties, "Default")
            });

            context.HttpContext.Response.Redirect(sourceUrl.ToString());
            return null;
        }

        protected virtual async Task<StageValidationContext> ValidateRedirectUriAsync(StageValidationContext context, ClientCredential client, IOauthServerProvider provider)
        {
            StageValidationContext redirectUriContext = new StageValidationContext(context);
            bool isValidated = await provider.ValidateRedirectUriAsync(redirectUriContext, client);
            if (!isValidated)
                await redirectUriContext.RejectAsync(ErrorCodes.invalid_request, Resources.Msg_InvalidRedirectUri);
            else
                await redirectUriContext.ValidateAsync();

            return redirectUriContext;
        }

        
        protected virtual async Task<StageValidationContext> HandleAuthorizationCodeAsync(StageValidationContext context, IOauthServerProvider provider)
        {
            StageValidationContext stageContext = new StageValidationContext(context);

            StageValidationContext validateClientContext = await ValidateClientCredentialsAsync(context, provider, true);
            if (!validateClientContext.IsValidated)
                return validateClientContext;

            StageValidationContext ValidateAuthorizationContext = new StageValidationContext(context);
            Authorization authorization = await provider.OnExchangeAuthorizationCode(context.Code);
            if (authorization == null)
                return await ValidateAuthorizationContext.RejectAsync(ErrorCodes.access_denied, Resources.Msg_InvalidCode);

            StageValidationContext validateRedirectUriContext = await ValidateRedirectUriAsync(validateClientContext, context.Client, provider);
            if(validateRedirectUriContext.IsValidated && !string.Equals(authorization.AuthenticationTicket.Properties.RedirectUri, context.RedirectUri, StringComparison.OrdinalIgnoreCase))
            {
                return await validateRedirectUriContext.RejectAsync(ErrorCodes.invalid_request, Resources.Msg_InvalidRedirectUri);
            }

            if (!validateRedirectUriContext.IsValidated)
                return validateRedirectUriContext;

            context.Scope = authorization.AuthenticationTicket.Properties.GetString("Scope").Split(" ".ToCharArray());
            StageValidationContext validateScopesContext = await ValidateScopeAsync(context, context.Client, provider);
            if (!validateScopesContext.IsValidated)
                return validateScopesContext;

            await CreateJwt(context, authorization.AuthenticationTicket, provider);

            await stageContext.ValidateAsync();
            return stageContext;
        }
    }
}
