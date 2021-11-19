namespace Faaast.Authentication.OAuth2
{
    /// <summary>
    /// Default values for the FaaastOauth authentication handler.
    /// </summary>
    public static class FaaastOauthDefaults
    {
        /// <summary>
        /// The default scheme for Oauth authentication. The value is <c>FaaastOauth</c>.
        /// </summary>
        public const string AuthenticationScheme = "FaaastOauth";

        /// <summary>
        /// The default display name for Oauth authentication. The value is <c>FaaastOauth</c>.
        /// </summary>
        public static readonly string DisplayName = "FaaastOauth";

        /// <summary>
        /// The default endpoint used to perform authentication.
        /// </summary>

        public static readonly string AuthorizationEndpoint = "/oauth/authorize";

        /// <summary>
        /// The endpoint used to retrieve access tokens.
        /// </summary>
        public static readonly string TokenEndpoint = "/oauth/token";

        /// <summary>
        /// The endpoint that is used to gather additional user information.
        /// </summary>      
        public static readonly string UserInformationEndpoint = "/oauth/user";

        /// <summary>
        /// The endpoint that is used to sign out.
        /// </summary>     
        public static readonly string SignOutEndpoint = "/oauth/logout";

    }
}
