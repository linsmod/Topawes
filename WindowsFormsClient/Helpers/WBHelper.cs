using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsClient.Models;
using WinFormsClient.WBMode;
using WinFormsClient.Extensions;
using Moonlight.WindowsForms.Controls;
using CSWebDownloader;
using TopModel;
using Moonlight.Treading;
using System.Diagnostics;
using Moonlight;

namespace WinFormsClient.Helpers
{
    public class WBHelper : IDisposable
    {
        public static BlockingQueue<Moonlight.WindowsForms.Controls.ExtendedWinFormsWebBrowser> wbQueue = new BlockingQueue<Moonlight.WindowsForms.Controls.ExtendedWinFormsWebBrowser>();
        public static BlockingQueue<Moonlight.WindowsForms.Controls.ExtendedWinFormsWebBrowser> ajaxWbQueue = new BlockingQueue<Moonlight.WindowsForms.Controls.ExtendedWinFormsWebBrowser>();
        public ExtendedWinFormsWebBrowser WB;
        internal static string Cookie;

        internal SynchronousNavigationContext SyncNavContext { get; private set; }
        internal static Control container;
        public static string wbInitUrl;
        public bool IsForAjaxUse;
        public bool IsNew;
        public static void InitWBHelper(Control parent, string initUrl)
        {
            container = parent;
            wbInitUrl = initUrl;
            int i = 0;
            foreach (var item in parent.Controls)
            {
                var wb = item as ExtendedWinFormsWebBrowser;
                if (wb != null)
                {
                    wb.ScriptErrorsSuppressed = true;
                    if (i % 2 == 0)
                    {
                        wbQueue.Enqueue(wb);
                    }
                    else
                    {
                        ajaxWbQueue.Enqueue(wb);
                    }
                    wb.Navigate(initUrl);
                    i++;
                }
            }
        }

        /// <summary>
        /// 开个新的浏览器
        /// </summary>
        public WBHelper()
        {
            container.InvokeAction(() =>
            {
                this.WB = new ExtendedWinFormsWebBrowser();
                container.Controls.Add(this.WB);
                this.WB.ScriptErrorsSuppressed = true;
                SubscribEvents();
                IsNew = true;
            });
        }

        public WBHelper(bool ajax)
        {
            IsForAjaxUse = ajax;
            if (AppSetting.UserSetting.Get("不限制浏览器数量", false))
            {
                if (wbQueue.Count == 0)
                {
                    container.InvokeAction(() =>
                    {
                        this.WB = new ExtendedWinFormsWebBrowser();
                        container.Controls.Add(this.WB);
                        this.WB.ScriptErrorsSuppressed = true;
                        SubscribEvents();
                        if (ajax)
                        {
                            this.WB.Navigate(wbInitUrl);
                            Task.Delay(50);
                        }
                    });
                    return;
                }
            }
            this.WB = ajax ? ajaxWbQueue.Dequeue() : wbQueue.Dequeue();
            SubscribEvents();
        }

        private void WB_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (SyncNavContext == null)
                return;
            if (!SyncNavContext.Tcs.Task.IsCompleted && SyncNavContext.EndUrls.Any(x => x == WB.Document.Url.AbsoluteUri))
            {
                if ((int)SyncNavContext.Tcs.Task.Status <= 3)
                {
                    SyncNavContext.Tcs.TrySetResult(SynchronousLoadResult.GetWebBrowserResult(true));
                }
                else
                {
                    SyncNavContext.Tcs.TrySetResult(SynchronousLoadResult.GetWebBrowserResult(false));
                }
            }
        }

        private void WB_NewWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var url = WB.Document.Url;
            e.Cancel = true;
            WB.Navigate(url);
        }

        private void WB_DownloadCompleted(object sender, CSWebDownloader.HttpDownloadCompletedEventArgs e)
        {
            var client = (sender as HttpDownloadClient);
            var url = client.Url;
            if (SyncNavContext.EndUrls.Any(x => x == url.AbsoluteUri) && e.Error == null)
            {
                SyncNavContext.Tcs.TrySetResult(SynchronousLoadResult.GetFileDownloadResult(true, client.DownloadPath));
            }
            else
            {
                SyncNavContext.Tcs.TrySetResult(SynchronousLoadResult.GetFileDownloadResult(false, client.DownloadPath));
            }
        }
        private static event Action LoginRequired;
        private void WB_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (SyncNavContext == null)
                return;
            if (e.Url.AbsoluteUri.StartsWith("https://login.taobao.com/member/login.jhtml"))
            {
                SyncNavContext.Tcs.TrySetResult(SynchronousLoadResult.GetWebBrowserResult(false, true));
                return;
            }
            if (!SyncNavContext.Tcs.Task.IsCompleted && SyncNavContext.EndUrls.Any(x => x == WB.Document.Url.AbsoluteUri))
            {
                if ((int)SyncNavContext.Tcs.Task.Status <= 3)
                {
                    SyncNavContext.Tcs.TrySetResult(SynchronousLoadResult.GetWebBrowserResult(true));
                }
                else
                {
                    SyncNavContext.Tcs.TrySetResult(SynchronousLoadResult.GetWebBrowserResult(false));
                }
            }
        }

        public async Task<DataLoadResult<HtmlDocument>> SynchronousLoadDocument(string url)
        {
            return await SynchronousLoadDocument(url, url);
        }
        public async Task<DataLoadResult<HtmlDocument>> SynchronousLoadDocument(string url, params string[] endUrls)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(Debugger.IsAttached ? 15 : 10));
            this.SyncNavContext = new SynchronousNavigationContext
            {
                StartUrl = url,
                EndUrls = endUrls,
                Tcs = new TaskCompletionSource<SynchronousLoadResult>(),
            };

            using (cts.Token.Register(() => SyncNavContext.Tcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                this.WB.Navigate(url);
                var result = await SyncNavContext.Tcs.Task;
                if (result.LoginRequired)
                {
                    return new DataLoadResult<HtmlDocument> { LoginRequired = true };
                }
                if (result.Success) // wait for DocumentCompleted
                {
                    if (result.IsWebBrowserResult)
                        return new DataLoadResult<HtmlDocument> { Data = WB.Document };
                    else
                        throw new NotSupportedException("无法将文件转换为HtmlElement");
                }
            }
            return new DataLoadResult<HtmlDocument> { Data = null };
        }

        public async Task<DataLoadResult<string>> SynchronousLoadString(string url)
        {
            return await SynchronousLoadString(url, url);
        }

        public async Task<DataLoadResult<string>> SynchronousLoadString(string url, params string[] endUrls)
        {
            this.SyncNavContext = new SynchronousNavigationContext
            {
                StartUrl = url,
                EndUrls = endUrls,
                Tcs = new TaskCompletionSource<SynchronousLoadResult>(),
            };
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(Debugger.IsAttached ? 15 : 10));
            using (cts.Token.Register(() => SyncNavContext.Tcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                this.WB.Navigate(url);
                var result = await SyncNavContext.Tcs.Task;
                cts.Dispose();
                if (result.LoginRequired)
                {
                    return new DataLoadResult<string>
                    {
                        LoginRequired = true
                    };
                }
                if (result.Success) // wait for DocumentCompleted
                {
                    if (result.IsWebBrowserResult)
                        return new DataLoadResult<string> { Data = WB.DocumentText };
                    else if (System.IO.File.Exists(result.FilePath))
                    {
                        var content = System.IO.File.ReadAllText(result.FilePath);
                        System.IO.File.Delete(result.FilePath);
                        return new DataLoadResult<string> { Data = content };
                    }
                }
            }
            return new DataLoadResult<string>
            {
                Data = string.Empty
            };

        }

        private void UnsubscribEvents()
        {
            WB.DownloadCompleted -= WB_DownloadCompleted;
            WB.DocumentCompleted -= WB_DocumentCompleted;
            WB.Navigated -= WB_Navigated;
        }

        private void SubscribEvents()
        {
            WB.DownloadCompleted += WB_DownloadCompleted;
            WB.DocumentCompleted += WB_DocumentCompleted;
        }

        public void Dispose()
        {
            UnsubscribEvents();
            if (IsNew)
            {
                container.InvokeAction(container.Controls.Remove, WB);
                WB = null;
            }
            else
            {
                if (IsForAjaxUse)
                    ajaxWbQueue.Enqueue(WB);
                else wbQueue.Enqueue(WB);
            }
        }

        public async Task<bool> IsLoginRequired()
        {
            var cnt = await SynchronousLoadString("http://chongzhi.taobao.com/index.do?method=info");
            return cnt.LoginRequired;
        }
    }
    public class DataLoadResult<T>
    {

        public T Data { get; set; }
        public bool LoginRequired { get; internal set; }
    }
}
