namespace Faaast.OAuth2Server.Configuration
{
    public interface IOauthBuilder
    {
        IOauthBuilder AddAuthorizationCodeGrantFlow();

        IOauthBuilder AddImplicitGrantFlow();

        IOauthBuilder AddResourceOwnerPasswordCredentialsGrantFlow();

        IOauthBuilder AddClientCredentialsGrantFlow();

        IOauthBuilder AddRefreshTokenFlow();
    }
}
