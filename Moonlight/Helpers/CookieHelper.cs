using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Moonlight.Helpers
{
    public class CookieHelper
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);
        ///
        /// 获取cookie
        ///

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetGetCookie(
         string url, string name, StringBuilder data, ref int dataSize);


        public static Dictionary<string, string> GetAllCookie(WebBrowser wb)
        {
            var dict = new Dictionary<string, string>();
            if (wb.Document != null && !string.IsNullOrEmpty(wb.Document.Cookie))
            {
                ////BIDUPSID=99D87FCA1350BE5DE1D2763DE05A5AAE; PSTM=1445567131; BAIDUID=B3E0C264723A911494367E073E026714:FG=1; H_PS_PSSID=17518_1461_17636_17619_12657_17640_14430_17000_17470_17073_15820_11868_17050; BD_UPN=112251; ISSW=1; H_PS_BBANNER=1; H_PS_645EC=13b7L22V5wTBBo78Ieggfxn8B0k3wFfCjN4Fk2sfe0qw7Lc1Thvu9Tr54J3Of5O%2F0lBd; sug=3; sugstore=1; ORIGIN=2; bdime=0; BD_LAST_QID=16121628640000643328; BDSVRTM=8; BD_HOME=0
                UpsertCookie(dict, wb.Document.Cookie);
                StringBuilder cookie = new StringBuilder(new String(' ', 2048));
                int datasize = cookie.Length;
                if (InternetGetCookie("http://" + wb.Document.Url.Host, null, cookie, ref datasize))
                {
                    //BIDUPSID=99D87FCA1350BE5DE1D2763DE05A5AAE; PSTM=1445567131; BAIDUID=B3E0C264723A911494367E073E026714:FG=1
                    var cookies = cookie.ToString();
                    UpsertCookie(dict, cookies);
                }
            }
            return dict;
        }
        public static void UpsertCookie(Dictionary<string, string> dict, string cookie)
        {
            var pairs = cookie.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var x = pair.Split(new char[] { '=' }, 2);
                if (x.Length == 2)
                {
                    dict[x[0]] = x[1];
                }
                else
                {
                    dict[x[0]] = "";
                }
            }
        }

        public static string ExpandCookieDictionary(Dictionary<string, string> dict)
        {
            return string.Join("; ", dict.Select(x => x.Key + "=" + x.Value));
        }

        public static void WriteWebBrowserCookie(string url, string cookie)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            UpsertCookie(dict, cookie);
            foreach (var item in dict)
            {
                InternetSetCookie(url, item.Key, item.Value);
            }
        }
    }
}
