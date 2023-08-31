using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Faaast.Authentication.OAuth2
{
    public static class HandlerUtils
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
                ValidAudience = options.Audience,
                ClockSkew = TimeSpan.FromMinutes(5),
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,
                ValidateLifetime = true,
                RequireSignedTokens = true,
                IssuerSigningKey = securityKey,
                ValidateIssuerSigningKey = true
            };
            var principal = tokenHandler.ValidateToken(accessToken, validationParams, out _);
            return principal;
        }

        internal static OAuthTokenResponse Parse(string json) =>
#if NETSTANDARD2_0 || NET461 || NET6_0
                OAuthTokenResponse.Success(JObject.Parse(json));
#elif NET5_0
                OAuthTokenResponse.Success(JsonDocument.Parse(json));
#endif

    }
}
