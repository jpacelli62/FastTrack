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
            Assert.False(options.SaveTokens);
            Assert.Contains(options.ClaimActions, x =>x.ClaimType == "All");
        }

        [Fact]
        public void Check_argumentEceptions()
        {
            AssertThrowsArgumentException(x => x.OauthServerUri = null);
            AssertThrowsArgumentException(x => x.AuthorizationEndpoint = null);
            AssertThrowsArgumentException(x => x.TokenEndpoint = null);
            AssertThrowsArgumentException(x => x.ClientId = null);
            AssertThrowsArgumentException(x => x.ClientSecret = null);
        }

        private static void AssertThrowsArgumentException(Action<FaaastOauthOptions> action)
        {
            var options = GetOptions();
            action(options);
            Assert.Throws<ArgumentException>(() => options.Validate());
        }
    }
}
