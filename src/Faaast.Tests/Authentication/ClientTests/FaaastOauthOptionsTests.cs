using System;
using System.Linq;
using System.Threading.Tasks;
using Faaast.Authentication.OAuth2;
using Xunit;

namespace Faaast.Tests.Authentication.ClientTests
{
    public class FaaastOauthOptionsTests
    {
        private static FaaastOauthOptions GetOptions()
        {
            var options = new FaaastOauthOptions
            {
                OauthServerUri = "https://my.domain.com",
                ClientId = "clientid",
                ClientSecret = "clientsecret",
                UseUserInformationEndpoint = true,
                SignOutScheme = "Cookies"
            };
            options.Validate();
            return options;
        }

        [Fact]
        public void Check_options_values()
        {
            var options = GetOptions();
            Assert.Equal("https://my.domain.com", options.OauthServerUri);
            Assert.Equal("https://my.domain.com/oauth/authorize", options.AuthorizationEndpoint);
            Assert.Equal("https://my.domain.com/oauth/token", options.TokenEndpoint);
            Assert.Equal("https://my.domain.com/oauth/user", options.UserInformationEndpoint);
            Assert.Equal("https://my.domain.com/oauth/logout", options.SignOutEndpoint);
            Assert.Equal("/faaastoauth/signin", options.CallbackPath.Value);
            Assert.Equal("Cookies", options.SignOutScheme);
            Assert.False(options.UsePkce);
            Assert.True(options.UseUserInformationEndpoint);
            Assert.True(options.SaveTokens);
            Assert.Contains(options.ClaimActions, x =>x.ClaimType == "All");
        }

        [Fact]
        public void Check_argumentEceptions()
        {
            this.AssertThrowsArgumentException(x => x.OauthServerUri = null);
            this.AssertThrowsArgumentException(x => x.AuthorizationEndpoint = null);
            this.AssertThrowsArgumentException(x => x.TokenEndpoint = null);
            this.AssertThrowsArgumentException(x => x.UserInformationEndpoint = null);
            this.AssertThrowsArgumentException(x => x.ClientId = null);
            this.AssertThrowsArgumentException(x => x.ClientSecret = null);
        }

        private void AssertThrowsArgumentException(Action<FaaastOauthOptions> action)
        {
            var options = GetOptions();
            action(options);
            Assert.Throws<ArgumentException>(() => options.Validate());
        }
    }
}
