using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Faaast.Authentication.OAuth2
{
    public static class HandlerExtensions
    {
        internal static string BuildUserPayload(ClaimsIdentity identity)
        {
            var excludeList = new[] { "iat", "exp", "iss", "nbf", "scope", "aud" };
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                foreach (var claim in identity.Claims.Where(c => !excludeList.Any(exclude => string.Equals(exclude, c.Type))))
                {
                    writer.WriteString(claim.Type, claim.Value);
                }

                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        internal static ClaimsPrincipal ReadPrincipalFromToken(string accessToken, FaaastOauthOptions options)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keybytes = Encoding.ASCII.GetBytes(options.ClientSecret);
            SecurityKey securityKey = new SymmetricSecurityKey(keybytes);
            var validationParams = new TokenValidationParameters
            {
                RequireAudience = true,
                ValidateAudience = true,
                RequireExpirationTime = true,
                ValidAudience = options.ClientId,
                ClockSkew = TimeSpan.FromMinutes(5),
                ValidateIssuer = false,
                ValidateLifetime = true,
                RequireSignedTokens = true,
                IssuerSigningKey = securityKey,
                ValidateIssuerSigningKey = true
            };
            var principal = tokenHandler.ValidateToken(accessToken, validationParams, out _);
            return principal;
        }

        internal static async Task<OAuthTokenResponse> CallRefreshTokenAsync(this HttpContext context, AuthenticateResult auth, FaaastOauthOptions options)
        {
            var refreshToken = auth.Properties.GetTokenValue("refresh_token");
            var endPoint = options.TokenEndpoint;
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
