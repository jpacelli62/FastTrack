using System;
using Microsoft.AspNetCore.Authentication;

namespace Faaast.OAuth2Server
{
    public class AuthorizationCode
    {
        public AuthenticationTicket Ticket { get; set; }

        public string Code { get; set; }

        public DateTimeOffset Expires { get; set; }
    }
}
