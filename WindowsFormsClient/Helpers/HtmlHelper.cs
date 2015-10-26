using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;

namespace WinFormsClient
{
    public class HtmlHelper
    {
        public static string GetText(Stream stream, Encoding encoding)
        {
            var streamReader = new StreamReader(stream, Encoding.GetEncoding("gb2312"));
            var html = streamReader.ReadToEnd();
            return html;
        }

        public static string UnicodeToGB2312(string str)
        {
            if (str.StartsWith("%5C", StringComparison.OrdinalIgnoreCase))
            {
                str = HttpUtility.UrlDecode(str);
            }
            if (!str.StartsWith("\\")) {
                return str;
            }
            string r = "";
            MatchCollection mc = Regex.Matches(str, @"\\u([\w]{2})([\w]{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            byte[] bts = new byte[2];
            foreach (Match m in mc)
            {
                bts[0] = (byte)int.Parse(m.Groups[2].Value, NumberStyles.HexNumber);
                bts[1] = (byte)int.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
                r += Encoding.Unicode.GetString(bts);
            }
            return r;
        }

        public static string GetDocumentText(WebBrowser wb)
        {
            if (!string.IsNullOrEmpty(wb.Document.Encoding))
            {
                return GetText(wb.DocumentStream, Encoding.GetEncoding(wb.Document.Encoding));
            }
            else if (!string.IsNullOrEmpty(wb.Document.DefaultEncoding))
            {
                return GetText(wb.DocumentStream, Encoding.GetEncoding(wb.Document.DefaultEncoding));
            }
            else
            {
                return GetText(wb.DocumentStream, Encoding.UTF8);
            }
        }
    }
}
