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
using Faaast.Authentication.OAuth2;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AuthenticationHttpContextExtensions
    {
        public static Task<string> GetAccessTokenAsync(this HttpContext context) => context.GetTokenAsync("access_token");

        public static Task<string> GetRefreshTokenAsync(this HttpContext context) => context.GetTokenAsync("refresh_token");
    }
}