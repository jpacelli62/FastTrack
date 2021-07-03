using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faaast.OAuth2Server.Configuration
{
    public class ClientCredential
    {
        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }
    }
}
