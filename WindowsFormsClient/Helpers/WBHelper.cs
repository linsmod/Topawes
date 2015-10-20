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
        CancellationTokenSource cancelDelayTokenSource = new CancellationTokenSource();
        internal static string Cookie;

        internal SynchronousNavigationContext SyncNavContext { get; private set; }

        public static void InitWBHelper(Control parent, string initUrl, int count)
        {
            if (wbQueue.Count <= 1)
            {
                int i = 0;
                foreach (var item in parent.Controls)
                {
                    var wb = item as ExtendedWinFormsWebBrowser;
                    if (wb != null)
                    {
                        wb.ScriptErrorsSuppressed = true;
                        if (i + 1 < count)
                        {
                            wbQueue.Enqueue(wb);
                        }
                        else
                        {
                            ajaxWbQueue.Enqueue(wb);
                            wb.Navigate(initUrl);
                        }
                        TaskEx.Delay(200).Wait();
                    }
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
            WB.DownloadCompleted += WB_DownloadCompleted;
            WB.DocumentCompleted += WB_DocumentCompleted;
            WB.Navigated += WB_Navigated;
        }

        private void WB_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
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

        private void WB_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
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

        public async Task<HtmlElement> SynchronousLoadDocument(string url)
        {
            return await SynchronousLoadDocument(url, url, 15, CancellationToken.None);
        }
        public async Task<HtmlElement> SynchronousLoadDocument(string url, string endUrl, int timeoutSeconds, CancellationToken token)
        {
            this.SyncNavContext = new SynchronousNavigationContext
            {
                StartUrl = url,
                EndUrl = endUrl,
                Tcs = new TaskCompletionSource<SynchronousLoadResult>(),
                TimeoutSeconds = timeoutSeconds,
            };

            using (token.Register(() => SyncNavContext.Tcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                SetTimeoutThread(SyncNavContext);
                this.WB.Cookie = Cookie;
                this.WB.Navigate(url);
                var result = await SyncNavContext.Tcs.Task;
                cancelDelayTokenSource.Cancel();

                if (result.Success) // wait for DocumentCompleted
                {
                    if (result.IsWebBrowserResult)
                        return WB.Document.Body;
                    else
                        throw new NotSupportedException("无法将文件转换为HtmlElement");
                }
            }
            return null;
        }
        public void SetTimeoutThread(SynchronousNavigationContext snc)
        {
            Task.Factory.StartNew(async () =>
            {
                var when = DateTime.Now.AddSeconds(snc.TimeoutSeconds);
                while (when > DateTime.Now && (int)snc.Tcs.Task.Status <= 3)
                {
                    if (cancelDelayTokenSource.IsCancellationRequested)
                    {
                        break;
                    }
                    await TaskEx.Delay(50);
                }
                if (snc.Tcs.Task.Status != TaskStatus.RanToCompletion)
                {
                    snc.Tcs.TrySetCanceled();
                }
            });
        }

        public async Task<string> SynchronousLoadString(string url)
        {
            return await SynchronousLoadString(url, CancellationToken.None);
        }

        public async Task<string> SynchronousLoadString(string url, CancellationToken token)
        {
            this.SyncNavContext = new SynchronousNavigationContext
            {
                StartUrl = url,
                EndUrl = url,
                Tcs = new TaskCompletionSource<SynchronousLoadResult>(),
                TimeoutSeconds = 60
            };

            using (token.Register(() => SyncNavContext.Tcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                SetTimeoutThread(SyncNavContext);
                this.WB.Cookie = Cookie;
                this.WB.Navigate(url);
                var result = await SyncNavContext.Tcs.Task;
                cancelDelayTokenSource.Cancel();
                if (result.Success) // wait for DocumentCompleted
                {
                    if (result.IsWebBrowserResult)
                        return WB.DocumentText;
                    else if (System.IO.File.Exists(result.FilePath))
                    {
                        var content = System.IO.File.ReadAllText(result.FilePath);
                        System.IO.File.Delete(result.FilePath);
                        return content;
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 查询供应商信息
        /// </summary>
        /// <param name="wb"></param>
        /// <param name="spu"></param>
        /// <returns></returns>
        public async Task<SuplierInfo> supplierInfo(string spu)
        {
            var url = "http://chongzhi.taobao.com/item.do?spu={0}&action=edit&method=supplierInfo";
            url = string.Format(url, spu);
            var content = await SynchronousLoadString(url, CancellationToken.None);
            var suplierInfo = JsonConvert.DeserializeObject<SuplierInfo>(content);
            if (suplierInfo.profitData != null && suplierInfo.profitData.Any())
            {
                //查到供应商
                return suplierInfo;
            }
            else
            {
                //无供应商,平台默认会自动关单
                return null;
            }
        }

        /// <summary>
        /// 设置供应商
        /// </summary>
        /// <param name="wb"></param>
        /// <param name="sup"></param>
        /// <param name="spu"></param>
        /// <param name="profitMin"></param>
        /// <param name="profitMax"></param>
        /// <param name="price"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public async Task<TaoJsonpResult> supplierSave(string sup, string spu, string profitMin, string profitMax, string price, string itemId, string tbcpCrumbs)
        {
            //profitMode=0 保证我赚钱
            //profitMode=2 自定义
            var url = "http://chongzhi.taobao.com/item.do?method=supplierSave&sup={0}&mode=2&spu={1}&itemId={2}&profitMode=2&profitMin={3}&profitMax={4}&price={5}&tbcpCrumbs={6}";
            url = string.Format(url, sup, spu, itemId, profitMin, profitMax, price, tbcpCrumbs);
            var content = await SynchronousLoadString(url, CancellationToken.None);
            return JsonConvert.DeserializeObject<TaoJsonpResult>(content);
        }

        /// <summary>
        /// 获取tbcpCrumbs参数
        /// </summary>
        /// <returns></returns>
        public async Task<string> GettbcpCrumbs()
        {
            var body = await SynchronousLoadDocument("http://chongzhi.taobao.com/item.do?method=list&type=1&keyword=&category=0&supplier=0&promoted=0&order=0&desc=0&page=1&size=20").ConfigureAwait(false);
            var tbcpCrumbs = body.JquerySelectInputHidden("tbcpCrumbs");
            return tbcpCrumbs;
        }

        public void Dispose()
        {
            WB.DownloadCompleted -= WB_DownloadCompleted;
            WB.DocumentCompleted -= WB_DocumentCompleted;
            WB.Navigated -= WB_Navigated;
            wbQueue.Enqueue(WB);
        }

        public async Task PrepareIfNoneDocument(string v)
        {
            if (WB.Document == null)
            {
                await this.SynchronousLoadString(v);
            }
        }
    }
}
