using System;
using System.Globalization;
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

        private string _oauthServerUri;
        public string OauthServerUri
        {
            get => _oauthServerUri;
            set
            {
                _oauthServerUri = value.TrimEnd('/');
                this.AuthorizationEndpoint = string.Concat(_oauthServerUri, FaaastOauthDefaults.AuthorizationEndpoint);
                this.TokenEndpoint = string.Concat(_oauthServerUri, FaaastOauthDefaults.TokenEndpoint);
                this.UserInformationEndpoint = string.Concat(_oauthServerUri, FaaastOauthDefaults.UserInformationEndpoint);
                this.SignOutEndpoint = string.Concat(_oauthServerUri, FaaastOauthDefaults.SignOutEndpoint);
            }
        }

        public string SignOutEndpoint { get; set; }
        public string SignOutScheme { get; set; }

        public bool UseUserInformationEndpoint { get; set; }

        /// <summary>
        /// Initializes a new <see cref="FaaastOauthOptions"/>.
        /// </summary>
        public FaaastOauthOptions()
        {
            this.CallbackPath = new PathString("/faaastoauth/signin");
            this.Scope.Add("identity");
            this.ClaimActions.MapAll();
            this.SaveTokens = true;
        }

        /// <summary>
        /// Check that the options are valid.  Should throw an exception if things are not ok.
        /// </summary>
        public override void Validate()
        {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
            if (string.IsNullOrEmpty(this.OauthServerUri))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(this.OauthServerUri)), nameof(this.OauthServerUri));
            }

            if (string.IsNullOrEmpty(this.AuthorizationEndpoint))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(this.AuthorizationEndpoint)), nameof(this.AuthorizationEndpoint));
            }

            if (string.IsNullOrEmpty(this.TokenEndpoint))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(this.TokenEndpoint)), nameof(this.TokenEndpoint));
            }

            if (string.IsNullOrEmpty(this.UserInformationEndpoint))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(this.UserInformationEndpoint)), nameof(this.UserInformationEndpoint));
            }

            if (string.IsNullOrEmpty(this.ClientId))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(this.ClientId)), nameof(this.ClientId));
            }

            if (string.IsNullOrEmpty(this.ClientSecret))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Exception_OptionMustBeProvided, nameof(this.ClientSecret)), nameof(this.ClientSecret));
            }
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 

            base.Validate();
        }
    }
}
