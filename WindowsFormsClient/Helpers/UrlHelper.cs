using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsClient
{
    public class UrlHelper
    {
        public static string GetStringValue(string url, string key, string defaultValue = "")
        {
            var pairArray = url.Split('&');
            var pairList = pairArray.Select(x => x.Split('=')).Where(x => x.Length == 2);
            foreach (var pair in pairList)
            {
                if (pair[0] == key)
                {
                    return pair[1];
                }
            }
            return defaultValue;
        }

        public static int GetIntValue(string url, string key, int defaultValue = 0)
        {
            url = new string(url.Skip(url.IndexOf('?')).ToArray());
            var pairArray = url.Split('&');
            var pairList = pairArray.Select(x => x.Split('=')).Where(x => x.Length == 2);
            foreach (var pair in pairList)
            {
                if (pair[0] == key)
                {
                    return int.Parse(pair[1]);
                }
            }
            return defaultValue;
        }

        public static string GetQueryString(Dictionary<string, object> parameters)
        {
            return string.Join("&", parameters.Select(x => x.Key + "=" + x.Value));
        }

        public static string SetValue(string url, string key, string value)
        {
            var left = url.Substring(0, url.IndexOf('?') + 1);
            var right = new string(url.Skip(url.IndexOf('?') + 1).ToArray());
            var pairArray = right.Split('&');
            var pairList = pairArray.Select(x => x.Split('=')).Where(x => x.Length == 2);
            foreach (var pair in pairList)
            {
                if (pair[0] == key)
                {
                    left += "&" + pair[0] + "=" + value;
                }
                else
                {
                    left += "&" + pair[0] + "=" + pair[1];
                }
            }
            return left.Replace("?&", "?");
        }
    }
}
