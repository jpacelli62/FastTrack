using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Faaast.Authentication.OAuth2Server.Core
{
    public partial class OAuthServerMiddleware
    {
        protected virtual async Task<StageValidationContext> HandleClientCredentialsAsync(StageValidationContext context, IOauthServerProvider provider)
        {
            StageValidationContext stage = await ValidateClientCredentialsAsync(context, provider, true);
            if(stage.IsValidated)
            {
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, "-1"));
                claims.Add(new Claim(ClaimTypes.Role, context.ClientId));
                var identity = new ClaimsIdentity(claims, "identity.application");
                await CreateJwt(stage, new Microsoft.AspNetCore.Authentication.AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(identity), "Default"), provider);
            }
            return stage;
        }

        protected virtual async Task<StageValidationContext> ValidateClientCredentialsAsync(StageValidationContext context, IOauthServerProvider provider, bool validateSecret)
        {
            StageValidationContext stage = new StageValidationContext(context);

            Client client = null;
            if (validateSecret)
                client = await provider.ValidateCredentialsAsync(context.ClientId, context.ClientSecret);
            else
                client = await provider.ValidateCredentialsAsync(context.ClientId);


            if (client == null || client.Audience != stage.Audience)
            {
                return await stage.RejectAsync(ErrorCodes.invalid_request, Resources.Msg_InvalidClient);
            }

            stage.Client = client;
            return await stage.ValidateAsync();
        }
    }
}
