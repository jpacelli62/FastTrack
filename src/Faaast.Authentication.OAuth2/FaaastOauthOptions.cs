using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Faaast.Authentication.OAuth2
{
    /// <summary>
    /// Configuration options for <see cref="FaaastOauthHandler"/>.
    /// </summary>
    public class FaaastOauthOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="FaaastOauthOptions"/>.
        /// </summary>
        public FaaastOauthOptions()
        {
            CallbackPath = new PathString("/faaastoauth/signin");
            Scope.Add("identity");
            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
            ClaimActions.MapJsonKey(ClaimTypes.GivenName, "first_name");
            ClaimActions.MapJsonKey(ClaimTypes.Surname, "last_name");
        }

        /// <summary>
        /// Check that the options are valid.  Should throw an exception if things are not ok.
        /// </summary>
        public override void Validate()
        {
            if (string.IsNullOrEmpty(OauthServerUri))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(OauthServerUri)), nameof(OauthServerUri));
            }


            if (string.IsNullOrEmpty(AuthorizationEndpoint))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(AuthorizationEndpoint)), nameof(AuthorizationEndpoint));
            }

            if (string.IsNullOrEmpty(TokenEndpoint))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(TokenEndpoint)), nameof(TokenEndpoint));
            }

            if (string.IsNullOrEmpty(UserInformationEndpoint))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(UserInformationEndpoint)), nameof(UserInformationEndpoint));
            }

            if (string.IsNullOrEmpty(ClientId))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(ClientId)), nameof(ClientId));
            }

            if (string.IsNullOrEmpty(ClientSecret))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(ClientSecret)), nameof(ClientSecret));
            }

            base.Validate();
        }

        //
        // Résumé :
        //     Gets or sets the authentication scheme corresponding to the middleware responsible
        //     of persisting user's identity after a successful authentication. This value typically
        //     corresponds to a cookie middleware registered in the Startup class. When omitted,
        //     Microsoft.AspNetCore.Authentication.AuthenticationOptions.DefaultSignInScheme
        //     is used as a fallback value.
        public string SignOutScheme { get; set; }
        public string SignOutEndpoint{ get; set; }


        private string _oauthServerUri;
        public string OauthServerUri { 
            get => _oauthServerUri;
            set 
            {
                _oauthServerUri = value.TrimEnd('/');
                AuthorizationEndpoint = string.Concat(_oauthServerUri, FaaastOauthDefaults.AuthorizationEndpoint);
                TokenEndpoint = string.Concat(_oauthServerUri, FaaastOauthDefaults.TokenEndpoint);
                UserInformationEndpoint = string.Concat(_oauthServerUri, FaaastOauthDefaults.UserInformationEndpoint);
                SignOutEndpoint = string.Concat(_oauthServerUri, FaaastOauthDefaults.SignOutEndpoint);
            }
        }
    }
}
