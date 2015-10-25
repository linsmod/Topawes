using Microsoft.AspNet.SignalR.Client;
using Moonlight.WindowsForms.Controls;
using mshtml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Top.Api.Request;
using WinFormsClient.BizEventArgs;
using WinFormsClient.WBMode;

namespace WinFormsClient
{
    /// <summary>
    /// 淘充值
    /// </summary>
    public class WBTaoChongZhiBrowserState : WebBrowserMode<WBTaoChongZhiBrowserState>
    {

        public event Action AskLogin;

        public string[] AllowedUrls { get; set; }
        public string[] IgnoreUrlList { get; set; }
        public string IndexURL = "http://chongzhi.taobao.com/index.do?method=index";
        public string StockURL = "http://chongzhi.taobao.com/stock.do?method=index";
        public const string ItemURL = "http://chongzhi.taobao.com/item.do?method=list&type=0&keyword=&category=0&supplier=0&promoted=0&order=0&desc=0&page=1&size=100";
        public string AccountURL = "http://chongzhi.taobao.com/account.do?method=index";
        public string TradeURL = "http://chongzhi.taobao.com/trade.do?method=index";

        public List<string> CookieHeaders = new List<string>();
        public WBTaoChongZhiBrowserState()
        {
            this.AllowedUrls = new string[] {
                "http://chongzhi.taobao.com/stock.do",//我要进货
                "http://chongzhi.taobao.com/item.do",//宝贝管理
                "http://chongzhi.taobao.com/account.do",//账户管理
                "http://chongzhi.taobao.com/trade.do",//交易管理
                "http://chongzhi.taobao.com/index.do" //首页
            };
            this.IgnoreUrlList = new string[] {
                "about:blank",
                "http://mpp.taobao.com",
                "http://123.56.122.122:8080/"
            };
        }

        private void OnAskLogin()
        {
            var h = AskLogin;
            if (h != null)
                h.Invoke();
            else
                throw new Exception("没有为事件AskLogin注册处理程序");
        }

        protected override void OnNavigating(WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.AbsoluteUri.StartsWith("https://login.taobao.com") || e.Url.AbsoluteUri.StartsWith("http://login.taobao.com"))
            {

            }
            else if (e.Url.AbsoluteUri.StartsWith("http://chongzhi.taobao.com"))
            {
            }
            else if (e.Url.AbsoluteUri.StartsWith("http://mpp.taobao.com"))
            {

            }
            else
            {
                e.Cancel = true;
            }
        }

        protected override void OnDocumentCompleted(string html, string url)
        {
            if (IgnoreUrlList.Any(x => url.StartsWith(x)))
            {
                return;
            }
            else if (url.StartsWith("https://login.taobao.com/member/login.jhtml"))
            {
                this.OnAskLogin();
            }
            else
            {
                var doc = (HTMLDocument)WB.Document.DomDocument;
                var head = (IHTMLDOMNode)doc.getElementById("hd");
                if (head != null)
                {
                    head.removeNode(true);
                }
                var foot = (IHTMLDOMNode)doc.getElementById("footer");
                if (foot != null) {
                    foot.removeNode(true);
                }
            }
        }

        protected override void EnterModeInternal(ExtendedWinFormsWebBrowser webBrowser)
        {
        }

        protected override void LeaveModeInternal(ExtendedWinFormsWebBrowser webBrowser)
        {
        }
    }
}
