using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public static class WebBrowserExtension
    {
        public static async Task<string> GetAjaxResult(this WebBrowser wb, string url)
        {
            wb.InstallAjaxMethod();
            wb.Document.InvokeScript("topAjax", new object[] { url });
            await Task.Delay(100);
            object content = null;
            while ((content = wb.Document.InvokeScript("getAjaxResult")) == null || content.ToString() == "")
            {
                await Task.Delay(100);
                if (wb.IsBusy)
                    continue;
            }
            return content.ToString();
        }

        private static void InstallAjaxMethod(this WebBrowser wb)
        {
            string content = "\r\n                function topAjax(url) {\r\n                var oAjax = null;\r\n                if (window.XMLHttpRequest)\r\n                {\r\n                    oAjax = new XMLHttpRequest();\r\n                }\r\n                else\r\n                {\r\n                    oAjax = new ActiveXObject('Microsoft.XMLHTTP');\r\n                }\r\n                oAjax.open('GET', url, true);\r\n                oAjax.send();\r\n                oAjax.onreadystatechange = function() {\r\n                    if (oAjax.readyState == 4)\r\n                    {\r\n                        if (oAjax.status == 200)\r\n                        {\r\n                            handleAjaxResult(oAjax.responseText);\r\n                        }\r\n                    }\r\n                };\r\n            }\r\n            function handleAjaxResult(d) {\r\n                var cnt = document.getElementById('ajaxResult');\r\n                if (cnt == null)\r\n                {\r\n                    cnt = document.createElement('div');\r\n                    cnt.setAttribute('id', 'ajaxResult');\r\n                    document.body.appendChild(cnt);\r\n                }\r\n                cnt.innerText = d;\r\n            };\r\n\r\n            function getAjaxResult() {\r\n                var cnt = document.getElementById('ajaxResult');\r\n                if (cnt == null)\r\n                {\r\n                    return '';\r\n                }\r\n                else\r\n                {\r\n                    var content = cnt.innerText;\r\n                    document.getElementById('ajaxResult').removeNode();\r\n                    return content;\r\n                }\r\n            };";
            wb.AppendJsElement("ajaxMethod", content);
        }

        private static void AppendJsElement(this WebBrowser wb, string id, string content)
        {
            HtmlElement JSElement = wb.Document.GetElementById(id);
            if (JSElement == null)
            {
                JSElement = wb.Document.CreateElement("script");
                JSElement.SetAttribute("type", "text/javascript");
                JSElement.SetAttribute("id", id);
                JSElement.SetAttribute("text", content);
                wb.Document.Body.AppendChild(JSElement);
            }
        }
    }
}
