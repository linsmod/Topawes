using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
namespace Moonlight.Helpers
{
    public class HttpHelper
    {
        public static Dictionary<string, string> ReadResponseHeaders(string url)
        {
            var dic = new Dictionary<string, string>();
            // Check the file information on the remote server.
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Credentials = CredentialCache.DefaultCredentials;
            using (var response = webRequest.GetResponse())
            {
                foreach (string header in response.Headers.Keys)
                {
                    dic.Add(header, response.Headers[header]);
                }
            }
            webRequest.Abort();
            webRequest = null;
            return dic;
        }
    }
}
