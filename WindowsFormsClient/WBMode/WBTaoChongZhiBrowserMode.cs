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
    public class WBTaoChongZhiBrowserMode : WebBrowserMode<WBTaoChongZhiBrowserMode>
    {
        /// <summary>
        /// 没加入淘充值
        /// </summary>
        public event EventHandler NotJoinTaoChongZhi;
        /// <summary>
        /// 导航到我要进货
        /// </summary>
        public event EventHandler<StockBizEventArgs> StockNavigated;
        /// <summary>
        /// 导航到宝贝管理
        /// </summary>
        public event EventHandler<ItemBizEventArgs> ItemNavigated;
        /// <summary>
        /// 导航到账户管理
        /// </summary>
        public event EventHandler<AccountBizEventArgs> AccountNavigated;
        /// <summary>
        /// 导航到交易管理
        /// </summary>
        public event EventHandler<TradeBizEventArgs> TradeNavigated;

        /// <summary>
        /// 导航到淘充值首页
        /// </summary>
        public event EventHandler<IndexBizEventArgs> IndexNavigated;

        public event Action AskLogin;

        public string[] AllowedUrls { get; set; }
        public string[] IgnoreUrlList { get; set; }
        public string IndexURL = "http://chongzhi.taobao.com/index.do?method=index";
        public string StockURL = "http://chongzhi.taobao.com/stock.do?method=index";
        public const string ItemURL = "http://chongzhi.taobao.com/item.do?method=list&type=0&keyword=&category=0&supplier=0&promoted=0&order=0&desc=0&page=1&size=100";
        public string AccountURL = "http://chongzhi.taobao.com/account.do?method=index";
        public string TradeURL = "http://chongzhi.taobao.com/trade.do?method=index";

        public List<string> CookieHeaders = new List<string>();
        public WBTaoChongZhiBrowserMode()
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



        /// <summary>
        /// 首页
        /// </summary>
        public void NavigateToIndexURL()
        {
            this.WB.Navigate(IndexURL);
        }

        private void OnAskLogin()
        {
            var h = AskLogin;
            if (h != null)
                h.Invoke();
            else
                throw new Exception("没有为事件AskLogin注册处理程序");
        }

        /// <summary>
        /// 我要进货
        /// </summary>
        public void NavigateToStockURL()
        {
            this.Navigate(StockURL);
        }

        /// <summary>
        /// 宝贝管理
        /// </summary>
        public void NavigatedToItemURL()
        {
            this.Navigate(ItemURL);
        }

        /// <summary>
        /// 账户管理
        /// </summary>
        public void NavigatedToAccountURL()
        {
            this.Navigate(AccountURL);
        }

        /// <summary>
        /// 交易管理
        /// </summary>
        public void NavigatedToTradeURL()
        {
            this.Navigate(TradeURL);
        }
        protected override void OnNavigating(WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.AbsoluteUri.StartsWith("https://login.taobao.com/member/login.jhtml"))
            {
                return;
            }

            //仅允许导航到指定链接
            var url = e.Url.ToString();
            if (!AllowedUrls.Any(x => url.StartsWith(x)) && !url.StartsWith("http://mpp.taobao.com/"))
            {
                e.Cancel = true;
                if (url != "about:blank")
                    Process.Start(url); //使用外部程序打开指定链接之外的URL
            }
        }

        protected override void OnDocumentCompleted(string html, string url)
        {
            if (IgnoreUrlList.Any(x => url.StartsWith(x)))
            {
                return;
            }

            if (url.StartsWith("http://chongzhi.taobao.com/stock.do"))
            {
                //我要进货
                var hStock = this.StockNavigated;
                if (hStock != null)
                {
                    hStock(this, StockBizEventArgs.FromHtml(html));
                }
            }
            else if (url.StartsWith("http://chongzhi.taobao.com/item.do"))
            {
                //宝贝管理
                var hItem = this.ItemNavigated;

                if (hItem != null)
                {
                    hItem(this, ItemBizEventArgs.FromWebBrowser(WB));
                }
            }
            else if (url.StartsWith("http://chongzhi.taobao.com/account.do"))
            {
                //账户管理
                var hAccount = this.AccountNavigated;
                if (hAccount != null)
                {
                    hAccount(this, AccountBizEventArgs.FromHtml(html));
                }
            }
            else if (url.StartsWith("http://chongzhi.taobao.com/trade.do"))
            {
                //交易管理
                var hTrade = this.TradeNavigated;
                if (hTrade != null)
                {
                    hTrade(this, TradeBizEventArgs.FromHtml(html));
                }
            }
            else if (url.StartsWith("http://chongzhi.taobao.com/index.do"))
            {
                //淘充值首页
                var hIndex = this.IndexNavigated;
                if (hIndex != null)
                {
                    hIndex(this, IndexBizEventArgs.FromHtml(html));
                }
            }
            else
            {
                if (html.IndexOf("账户专区") != -1)
                {
                    this.NavigatedToItemURL();
                }
                if (html.IndexOf("选择其中一个已登录的账户") != -1)
                {
                    this.OnAskLogin();
                    ////帮着点了
                    //var btnId = "J_SubmitQuick";
                    //WB.Document.All[btnId].InvokeMember("click");
                }
                else if (url.StartsWith("https://login.taobao.com/member/login.jhtml"))
                {
                    this.OnAskLogin();
                }
                else
                {
                    //尚未加入淘宝充值
                    this.OnNotJoinTaoChongZhi();
                }
            }
        }

        private void OnNotJoinTaoChongZhi()
        {
            var h = this.NotJoinTaoChongZhi;
            if (h != null)
            {
                h(this, EventArgs.Empty);
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
