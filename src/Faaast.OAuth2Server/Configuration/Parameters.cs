using System.Net.Http;

namespace Faaast.OAuth2Server.Configuration
{
    public class Parameters
    {
        public string ParameterName { get; set; }

        public HttpMethod[] AllowedMethods { get; set; }

        public Parameters(string name, params HttpMethod[] allowedMethods)
        {
            this.ParameterName = name;
            this.AllowedMethods = allowedMethods;
        }

        public static readonly string ClientCredentials = "client_credentials";
        public static readonly string AuthorizationCode = "authorization_code";

        public static readonly Parameters GrantType = new("grant_type", HttpMethod.Post);
        public static readonly Parameters RefreshToken = new("refresh_token", HttpMethod.Post);
        public static readonly Parameters Password = new("password", HttpMethod.Post);
        public static readonly Parameters UserName = new("username", HttpMethod.Post);
        public static readonly Parameters Scope = new("scope", HttpMethod.Post, HttpMethod.Get);
        public static readonly Parameters ClientId = new("client_id", HttpMethod.Post, HttpMethod.Get);
        public static readonly Parameters ClientSecret = new("client_secret", HttpMethod.Post);
        public static readonly Parameters RedirectUri = new("redirect_uri", HttpMethod.Post, HttpMethod.Get);
        public static readonly Parameters Audience = new("audience", HttpMethod.Post);
        public static readonly Parameters Code = new("code", HttpMethod.Post);
        public static readonly Parameters ResponseType = new("response_type", HttpMethod.Get);
        public static readonly Parameters State = new("state", HttpMethod.Get);
        public static readonly Parameters AppSecretProof = new("appsecret_proof", HttpMethod.Get);
        public static readonly Parameters AccessToken = new("access_token", HttpMethod.Post, HttpMethod.Get);
    }
}
