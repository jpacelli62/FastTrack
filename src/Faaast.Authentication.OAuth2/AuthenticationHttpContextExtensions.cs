using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Faaast.Authentication.OAuth2;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AuthenticationHttpContextExtensions
    {
        public static Task<string> GetAccessTokenAsync(this HttpContext context)
        {
            return context.GetTokenAsync("access_token");
        }

        public static Task<string> GetRefreshTokenAsync(this HttpContext context)
        {
            return context.GetTokenAsync("refresh_token");
        }

        internal static async Task<OAuthTokenResponse> CallRefreshTokenAsync(this HttpContext context, AuthenticateResult auth, FaaastOauthOptions options)
        {
            string refreshToken = auth.Properties.GetTokenValue("refresh_token");
            string endPoint = options.TokenEndpoint;
            var request = new HttpRequestMessage(HttpMethod.Post, endPoint);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", options.ClientId),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            });

            request.Content = content;
            var httpResult = await new HttpClient().SendAsync(request);
            if (httpResult.IsSuccessStatusCode)
            {
                var result = await httpResult.Content.ReadAsStringAsync();

#if NETSTANDARD2_0 || NET461
                return OAuthTokenResponse.Success(JObject.Parse(result));
#elif NET5_0
                return OAuthTokenResponse.Success(JsonDocument.Parse(result));
#endif
            }

            return null;
        }
    }
}