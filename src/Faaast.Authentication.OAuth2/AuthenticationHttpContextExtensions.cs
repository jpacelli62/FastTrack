using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AuthenticationHttpContextExtensions
    {
        public static Task<string> GetAccessTokenAsync(this HttpContext context) => context.GetTokenAsync("access_token");

        public static Task<string> GetRefreshTokenAsync(this HttpContext context) => context.GetTokenAsync("refresh_token");
    }
}