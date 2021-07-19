namespace Faaast.Authentication.OAuth2Server.Core
{
    public class ClientCredential
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Audience { get; set; }

        public string[] Scopes { get; set; }
    }
}
