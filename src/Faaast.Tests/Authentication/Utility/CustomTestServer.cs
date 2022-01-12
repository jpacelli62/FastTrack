using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Faaast.Tests.Authentication.Utility
{
    public class CustomTestServer
    {
        public IConfiguration Configuration { get; set; }

        public TestServer Server { get; set; }

        public ISystemClock Clock { get; set; }

        public Action<IServiceCollection> ConfigureServices { get; set; }

        public Action<IApplicationBuilder> Configure { get; set; }

        public CustomTestServer( )
        {
            this.Configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json")
                .Build();

            var clockMock = new Moq.Mock<ISystemClock>();
            var now = new DateTimeOffset(DateTime.UtcNow);
            clockMock.Setup(x => x.UtcNow).Returns(now);
            this.Clock = clockMock.Object;
        }

        public void Start(Uri baseAddress= null)
        {
            var builder = new WebHostBuilder()
              .Configure(app => this.Configure?.Invoke(app))
              .ConfigureServices(new Action<IServiceCollection>(services =>
              {
                  services.TryAddSingleton<ISystemClock>(this.Clock);
                  this.ConfigureServices?.Invoke(services);
              }));

            this.Server = new TestServer(builder)
            {
                BaseAddress = baseAddress
            };
        }


        public virtual async Task<Transaction> SendPostAsync(string uri, Dictionary<string, string> values, Action<HttpRequestMessage> configure = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var content = new FormUrlEncodedContent(values.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)));
            request.Content = content;
            configure?.Invoke(request);
            return await SendAsync(request);
        }

        public virtual async Task<Transaction> SendGetAsync(string uri, Dictionary<string, string> values, Action<HttpRequestMessage> configure = null)
        {
            var targetUri = string.Concat(
                uri,
                "?",
                string.Join('&', values.Select(x => string.Concat(x.Key, "=", System.Web.HttpUtility.UrlEncode(x.Value))).ToArray()));

            var request = new HttpRequestMessage(HttpMethod.Get, targetUri);
            configure?.Invoke(request);

            return await SendAsync(request);
        }

        public virtual async Task<Transaction> SendAsync(HttpRequestMessage request)
        {
            var transaction = new Transaction
            {
                Request = request,
                Response = await this.Server.CreateClient().SendAsync(request),
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
