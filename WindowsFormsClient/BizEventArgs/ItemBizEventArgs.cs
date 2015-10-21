using Moonlight.WindowsForms.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsClient.Extensions;
using WinFormsClient.HtmlEntity;

namespace WinFormsClient.BizEventArgs
{
    public class ItemBizEventArgs : EventArgs
    {
        Dictionary<string, HtmlElement> Items = new Dictionary<string, HtmlElement>();
        //http://chongzhi.taobao.com/item.do?spm=0.0.0.0.CNll5u&method=list&type=1&keyword=&category=0&supplier=0&promoted=0&order=0&desc=0&page=1&size=20
        public Dictionary<string, object> UrlParameters = new Dictionary<string, object>();
        public Dictionary<string, string> HiddenInputValues = new Dictionary<string, string>();
        public TableEntity TableEntity;
        public ExtendedWinFormsWebBrowser webBrowser { get; set; }
        public void NextPage()
        {
            UrlParameters["page"] = ((int)UrlParameters["page"]) + 1;
            var url = "http://chongzhi.taobao.com/item.do?" + UrlHelper.GetQueryString(UrlParameters);
            this.webBrowser.Navigate(url);
        }
        public static ItemBizEventArgs FromWebBrowser(ExtendedWinFormsWebBrowser webBrowser)
        {
            return null;
            //var args = new ItemBizEventArgs();
            //args.webBrowser = webBrowser;
            //var url = webBrowser.Url.ToString();

            //args.UrlParameters["type"] = UrlHelper.GetIntValue(url, "type");
            //args.UrlParameters["keyword"] = UrlHelper.GetStringValue(url, "keyword");
            //args.UrlParameters["category"] = UrlHelper.GetIntValue(url, "category");
            //args.UrlParameters["supplier"] = UrlHelper.GetIntValue(url, "supplier");
            //args.UrlParameters["promoted"] = UrlHelper.GetIntValue(url, "promoted");
            //args.UrlParameters["order"] = UrlHelper.GetIntValue(url, "order");
            //args.UrlParameters["desc"] = UrlHelper.GetIntValue(url, "desc");
            //args.UrlParameters["page"] = UrlHelper.GetIntValue(url, "page");
            //args.UrlParameters["size"] = UrlHelper.GetIntValue(url, "size");

            //var tables = webBrowser.Document.All.JQuerySelect("table.stock-table");
            //var hiddens = webBrowser.Document.Body.JQuerySelect("input[type=hidden]");
            //foreach (var item in hiddens)
            //{
            //    var name = string.IsNullOrEmpty(item.Name) ? item.Id : item.Name;
            //    args.HiddenInputValues[name] = item.GetAttribute("value");
            //}
            //if (tables.Any())
            //{
            //    //args.TableEntity = new TableEntity(tables[0]);
            //}
            //return args;
        }
    }
}
