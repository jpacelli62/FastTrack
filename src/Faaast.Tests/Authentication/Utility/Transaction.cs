using System.Net.Http;
using System.Xml.Linq;

namespace Faaast.Tests.Authentication
{
    public class Transaction
    {
        public HttpRequestMessage Request { get; set; }
        public HttpResponseMessage Response { get; set; }
        public string ResponseText { get; set; }
        public XElement ResponseElement { get; set; }
    }
}
