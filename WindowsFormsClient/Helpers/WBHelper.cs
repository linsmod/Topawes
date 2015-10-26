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

namespace WinFormsClient.Helpers
{
    public class WBHelper : IDisposable
    {
        public static BlockingQueue<Moonlight.WindowsForms.Controls.ExtendedWinFormsWebBrowser> wbQueue = new BlockingQueue<Moonlight.WindowsForms.Controls.ExtendedWinFormsWebBrowser>();
        public static BlockingQueue<Moonlight.WindowsForms.Controls.ExtendedWinFormsWebBrowser> ajaxWbQueue = new BlockingQueue<Moonlight.WindowsForms.Controls.ExtendedWinFormsWebBrowser>();
        public ExtendedWinFormsWebBrowser WB;
        internal static string Cookie;

        internal SynchronousNavigationContext SyncNavContext { get; private set; }

        public static void InitWBHelper(Control parent, string initUrl)
        {
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

        public WBHelper(bool ajax)
        {
            ExtendedWinFormsWebBrowser exWb = wbQueue.Dequeue();
            if (exWb.Parent == null)
            {
                throw new Exception("在使用前必须将浏览器放入某个控件");
            }
            this.WB = exWb;
            SubscribEvents();
        }

        private void WB_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (SyncNavContext == null)
                return;
            if (!SyncNavContext.Tcs.Task.IsCompleted && SyncNavContext.EndUrl == WB.Document.Url.AbsoluteUri)
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
            if (SyncNavContext.EndUrl == url.AbsoluteUri && e.Error == null)
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
            if (!SyncNavContext.Tcs.Task.IsCompleted && SyncNavContext.EndUrl == WB.Document.Url.AbsoluteUri)
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
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(15));
            return await SynchronousLoadDocument(url, url, cts.Token);
        }
        public async Task<DataLoadResult<HtmlDocument>> SynchronousLoadDocument(string url, string endUrl, CancellationToken token)
        {
            this.SyncNavContext = new SynchronousNavigationContext
            {
                StartUrl = url,
                EndUrl = endUrl,
                Tcs = new TaskCompletionSource<SynchronousLoadResult>(),
            };

            using (token.Register(() => SyncNavContext.Tcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                this.WB.Cookie = Cookie;
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
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(15));
            return await SynchronousLoadString(url, cts.Token);
        }

        public async Task<DataLoadResult<string>> SynchronousLoadString(string url, CancellationToken token)
        {
            this.SyncNavContext = new SynchronousNavigationContext
            {
                StartUrl = url,
                EndUrl = url,
                Tcs = new TaskCompletionSource<SynchronousLoadResult>(),
            };

            using (token.Register(() => SyncNavContext.Tcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                this.WB.Cookie = Cookie;
                this.WB.Navigate(url);
                var result = await SyncNavContext.Tcs.Task;
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
            wbQueue.Enqueue(WB);
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
