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
            CallbackPath = new PathString("/signin-oauth");
            SendAppSecretProof = true;
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

        /// <summary>
        /// Gets or sets if the <c>appsecret_proof</c> should be generated and sent with calls.
        /// </summary>
        /// <value>Defaults to <see langword="true"/>.</value>
        public bool SendAppSecretProof { get; set; }

        private string _oauthServerUri;
        public string OauthServerUri { 
            get => _oauthServerUri;
            set 
            {
                _oauthServerUri = value.TrimEnd('/');
                AuthorizationEndpoint = string.Concat(_oauthServerUri, FaaastOauthDefaults.AuthorizationEndpoint);
                TokenEndpoint = string.Concat(_oauthServerUri, FaaastOauthDefaults.TokenEndpoint);
                UserInformationEndpoint = string.Concat(_oauthServerUri, FaaastOauthDefaults.UserInformationEndpoint);
            }
        }
    }
}
