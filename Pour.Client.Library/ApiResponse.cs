using System.Collections.Generic;
using System.Net;

namespace Pour.Client.Library
{
    internal struct ApiResponse
    {
        internal bool Succedded;

        internal string ErrorMessage;

        internal HttpStatusCode Status;

        internal IDictionary<string, string> Headers;

        internal string ResponseBody;
    }
}
