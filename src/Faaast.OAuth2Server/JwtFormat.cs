//using Microsoft.AspNetCore.Authentication;
//using Microsoft.IdentityModel.Tokens;
//using System;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace Faaast.Authentication.OAuth2Server
//{
//    public class JwtFormat
//    {
//        public static string GenerateToken(ClaimsPrincipal principal, string jti, string issuer, string audience, string[] scopes, string clientId, string clientSecret, TimeSpan lifetime)
//        {
//            var securityKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(clientSecret));
//            var signingCredentials = new SigningCredentials(
//                securityKey,
//                SecurityAlgorithms.HmacSha256);

//            var header = new JwtHeader(signingCredentials);
//            DateTimeOffset? issued = new DateTimeOffset(DateTime.UtcNow);
//            DateTimeOffset? expires = issued.Value.AddMinutes(lifetime.TotalMinutes);
//            DateTime constant = new DateTime(1970, 1, 1);
//            JwtPayload payload = new JwtPayload
//            {
//                {JwtRegisteredClaimNames.Iss, issuer},
//                {JwtRegisteredClaimNames.Iat, (int)(issued.Value.UtcDateTime - constant).TotalSeconds},
//                {JwtRegisteredClaimNames.Jti, jti},
//                {JwtRegisteredClaimNames.Exp, (int)(expires.Value.UtcDateTime - constant).TotalSeconds },
//                {JwtRegisteredClaimNames.Aud, audience },
//                { "clientId", clientId },
//                { "scopes", scopes }
//            };
//            payload.AddClaims(principal.Claims);

//            JwtSecurityToken token = new JwtSecurityToken(header, payload);
//            var handler = new JwtSecurityTokenHandler();
//            var jwt = handler.WriteToken(token);
//            return jwt;
//        }
//    }
//}
