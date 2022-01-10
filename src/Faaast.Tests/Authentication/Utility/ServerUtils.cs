using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Faaast.Tests.Authentication.Utility
{
    public static class ServerUtils
    {
        public static TestServer CreateServer(Action<IApplicationBuilder> configureApp, Action<IServiceCollection> configureOptions, Uri baseAddress = null)
        {
            var builder = new WebHostBuilder()
                .Configure(configureApp)
                .ConfigureServices(configureOptions);

            var server = new TestServer(builder)
            {
                BaseAddress = baseAddress
            };

            return server;
        }

        public static async Task<Transaction> SendPostAsync(TestServer server, string uri, Dictionary<string, string> values, Action<HttpRequestMessage> configure = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var content = new FormUrlEncodedContent(values.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)));
            request.Content = content;
            configure?.Invoke(request);
            return await SendAsync(server, request);
        }

        public static async Task<Transaction> SendGetAsync(TestServer server, string uri, Dictionary<string, string> values, Action<HttpRequestMessage> configure = null)
        {
            var targetUri = string.Concat(
                uri,
                "?",
                string.Join('&', values.Select(x => string.Concat(x.Key, "=", System.Web.HttpUtility.UrlEncode(x.Value))).ToArray()));

            var request = new HttpRequestMessage(HttpMethod.Get, targetUri);
            configure?.Invoke(request);

            return await SendAsync(server, request);
        }

        public static async Task<Transaction> SendAsync(TestServer server, HttpRequestMessage request)
        {
            var transaction = new Transaction
            {
                Request = request,
                Response = await server.CreateClient().SendAsync(request),
            };

            transaction.ResponseText = await transaction.Response.Content.ReadAsStringAsync();

            if (transaction.Response.Content != null &&
                transaction.Response.Content.Headers.ContentType != null &&
                transaction.Response.Content.Headers.ContentType.MediaType == "text/xml")
            {
                transaction.ResponseElement = XElement.Parse(transaction.ResponseText);
            }

            return transaction;
        }
    }
}
