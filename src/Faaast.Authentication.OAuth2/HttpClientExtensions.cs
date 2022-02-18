using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public static class HttpClientExtensions
    {
        internal static async Task<string> CallRefreshTokenAsync(this HttpClient client, string url, string clientId, string refreshToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            });

            request.Content = content;
            var httpResult = await client.SendAsync(request);
            if (httpResult.IsSuccessStatusCode)
            {
                return await httpResult.Content.ReadAsStringAsync();
            }

            return null;
        }
    }
}
