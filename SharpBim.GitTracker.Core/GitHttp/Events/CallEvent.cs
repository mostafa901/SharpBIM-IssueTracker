using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharpBIM.GitTracker.Core.GitHttp.Events
{
    public class CallEventArgs : EventArgs
    {
        public CallEventArgs(HttpMethod method, string url, string body, HttpResponseMessage responseMsg, string response)
        {
            Method = method;
            Url = url;
            Body = body;
            ResponseMsg = responseMsg;
            Response = response;
        }

        public HttpMethod Method { get; }
        public string Url { get; }
        public string Body { get; }
        public HttpResponseMessage ResponseMsg { get; }
        public string Response { get; }
    }
}