namespace Faaast.OAuth2Server.Configuration
{
    public interface IOauthBuilder
    {
        IOauthBuilder AddAuthorizationCodeFlow();

        //IOauthBuilder AddImplicitGrant();

        IOauthBuilder AddResourceOwnerPasswordCredentialsFlow();

        IOauthBuilder AddClientCredentialsGrant();
    }
}
