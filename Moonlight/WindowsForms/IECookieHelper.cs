using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Moonlight
{
    public class IECookieHelper
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetGetCookieEx(string pchURL, string pchCookieName, StringBuilder pchCookieData, ref uint pcchCookieData, int dwFlags, IntPtr lpReserved);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);

        const int INTERNET_COOKIE_HTTPONLY = 0x00002000;
        public static string GetGlobalCookies(string uri, string name = null)
        {
            uint datasize = 1024;
            StringBuilder cookieData = new StringBuilder((int)datasize);
            if (InternetGetCookieEx(uri, name, cookieData, ref datasize, INTERNET_COOKIE_HTTPONLY, IntPtr.Zero)
            && cookieData.Length > 0)
            {
                return cookieData.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetGlobalCookie(string uri, string name)
        {
            var cookies = GetGlobalCookies(uri).Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var cookie in cookies)
            {
                var nv = cookie.Split(new char[] { '=' }, 2);
                if (nv.Length == 2 && nv[0] == name)
                {
                    return nv[1];
                }
            }
            return string.Empty;
        }


        public static void SetGlobalCookies(string uri, params string[] cookieHeader)
        {
            var nvList = cookieHeader.Select(x => x.Split('='));
            foreach (var item in nvList)
            {
                if (item.Length == 2)
                    InternetSetCookie(uri, item[0], item[1]);
            }
        }

        public static string GetCookieValueFromHeaderList(List<string> list, string name)
        {
            foreach (var item in list)
            {
                if (item.StartsWith(name) && item.Split('=').Length == 2)
                {
                    return item.Split('=')[1];
                }
            }
            return string.Empty;
        }

        public static List<string> GetCookieHeaderList(WebBrowser wb, List<string> list = null)
        {
            list = list ?? new List<string>();
            try
            {
                if (wb.Document != null)
                {
                    var cookieDict = new Dictionary<string, string>();
                    var cookies = wb.Document.Cookie == null ? list : wb.Document.Cookie.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    var fullUrl = wb.Document.Url.AbsoluteUri;
                    var server = fullUrl.Substring(0, fullUrl.Length - wb.Document.Url.PathAndQuery.Length);
                    if (server.IndexOf(".taobao.com") != -1)
                    {
                        server = "https://taobao.com";
                    }
                    var cookies2 = GetGlobalCookies(server).Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    cookies = cookies.Concat(cookies2).ToList();
                    foreach (var cookie in cookies)
                    {
                        var nv = cookie.Split(new char[] { '=' }, 2);
                        if (nv.Length == 1)
                        {
                            if (cookieDict.ContainsKey(nv[0]) && cookieDict[nv[0]] == "") { }
                            else
                                cookieDict[nv[0]] = "";
                        }
                        else
                        {
                            if (cookieDict.ContainsKey(nv[0]) && cookieDict[nv[0]] == nv[1]) { }
                            else
                                cookieDict[nv[0]] = nv[1]; //[update] isg,i,_cc_,uc1,existShop,[add] uc1,existShop,sg,_l_g_,_nk_,[update_in_new_page] uc1
                        }
                    }
                    list = cookieDict.ToList().Select(x => x.Key + "=" + x.Value).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return list;
        }

        //public static List<string> LoadCookieHeadersFile()
        //{
        //    var output = new List<string>();
        //    try
        //    {
        //        if (File.Exists("cookie.data"))
        //        {
        //            var lines = File.ReadAllLines("cookie.data");
        //            foreach (var line in lines)
        //            {
        //                var plain = Encryption.Decrypt(line, Encryption.DefaultKey);
        //                output.Add(plain);
        //            }
        //        }
        //    }
        //    catch (Exception ex) { }
        //    return output.Distinct().ToList();
        //}

        //public static void SaveCookieHeadersFile(List<string> values)
        //{
        //    var lines = new List<string>();
        //    foreach (var item in values.Distinct())
        //    {
        //        lines.Add(Encryption.Encrypt(item, Encryption.DefaultKey));
        //    }
        //    System.IO.File.WriteAllLines("cookie.data", lines);
        //}
    }
}
