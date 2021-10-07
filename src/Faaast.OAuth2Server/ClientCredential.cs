namespace Faaast.Authentication.OAuth2Server
{
    public class Client
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string[] Scopes { get; set; }

        public string Audience { get; set; }

    }
}
