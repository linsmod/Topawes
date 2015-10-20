using Microsoft.Phone.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsClient.WBMode
{
    public class SynchronousLoadResult
    {
        private SynchronousLoadResult() { }
        public static SynchronousLoadResult GetWebBrowserResult(bool success)
        {
            return new SynchronousLoadResult
            {
                Success = success,
                IsWebBrowserResult = true,
                IsFileDownloadResult = false
            };
        }
        public static SynchronousLoadResult GetFileDownloadResult(bool success, string filePath)
        {
            return new SynchronousLoadResult
            {
                Success = success,
                IsFileDownloadResult = true,
                IsWebBrowserResult = false,
                FilePath = filePath
            };
        }
        public bool Success { get; private set; }
        public bool IsWebBrowserResult { get; private set; }
        public bool IsFileDownloadResult { get; private set; }
        public string FilePath { get; set; }
    }
    public sealed class SynchronousNavigationContext
    {
        public int POLL_DELAY = 250;
        public string StartUrl { get; set; }
        public TaskCompletionSource<SynchronousLoadResult> Tcs { get; set; }
        public string EndUrl { get; set; }
        public double TimeoutSeconds { get; set; }

        public DateTime StartAt = DateTime.Now;
    }
    public abstract class WebBrowserMode<T> : IWebBrowserMode where T : IWebBrowserMode
    {
        public static object syncObj = new object();
        public bool IsSynchronousNavigating { get; set; }
        SynchronousNavigationContext SyncNavContext;
        public bool IsActive { get; protected set; }
        internal ExtendedWinFormsWebBrowser WB;
        public WebBrowserMode()
        {
        }
        private void WB_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            if (e.CurrentProgress > 0 && e.CurrentProgress <= e.MaximumProgress)
            {
                OnProgressChanged((int)((double)e.CurrentProgress / (double)e.MaximumProgress * 100));
            }
            else
            {
                OnProgressChanged(0);
            }
        }

        protected abstract void EnterModeInternal(ExtendedWinFormsWebBrowser webBrowser);
        protected abstract void LeaveModeInternal(ExtendedWinFormsWebBrowser webBrowser);

        /// <summary>
        /// 进入该模式
        /// </summary>
        public void Enter(ExtendedWinFormsWebBrowser webBrowser)
        {
            webBrowser.ProgressChanged += WB_ProgressChanged;
            webBrowser.Navigating += WebBrowser_Navigating;
            webBrowser.NewWindow += WebBrowser_NewWindow;
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
            webBrowser.Navigated += WebBrowser_Navigated;
            webBrowser.Dock = DockStyle.Fill;
            this.EnterModeInternal(webBrowser);
            WB = webBrowser;
            this.IsActive = true;
        }

        private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (this.IsActive)
            {
                this.OnNavigated(e.Url.AbsoluteUri);
            }
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (this.IsActive)
            {
                var html = HtmlHelper.GetDocumentText(WB);
                var url = e.Url.AbsoluteUri;
                if (IsSynchronousNavigating)
                {
                    lock (syncObj)
                    {
                        if (IsSynchronousNavigating)
                        {
                            if (!SyncNavContext.Tcs.Task.IsCompleted && SyncNavContext.EndUrl == url)
                            {
                                if (DateTime.Now.Subtract(SyncNavContext.StartAt).TotalSeconds > SyncNavContext.TimeoutSeconds)
                                {
                                    SyncNavContext.Tcs.TrySetResult(SynchronousLoadResult.GetWebBrowserResult(false));
                                }
                                else if (SyncNavContext.Tcs.Task.Status != TaskStatus.Canceled
                                    && SyncNavContext.Tcs.Task.Status != TaskStatus.Faulted
                                    && SyncNavContext.Tcs.Task.Status != TaskStatus.RanToCompletion)
                                {
                                    SyncNavContext.Tcs.TrySetResult(SynchronousLoadResult.GetWebBrowserResult(true));
                                }
                                IsSynchronousNavigating = false;
                            }
                        }
                    }

                }
                Application.DoEvents();
                if (!WB.IsDisposed)
                    OnDocumentCompleted(html, url);
            }
        }

        private void WebBrowser_NewWindow(object sender, CancelEventArgs e)
        {
            if (this.IsActive)
            {
                this.OnNewWindow(e);
            }
        }

        private void WebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (this.IsActive)
            {
                OnNavigating(e);
            }
        }

        /// <summary>
        /// 离开该模式
        /// </summary>
        public void Leave(ExtendedWinFormsWebBrowser webBrowser)
        {
            webBrowser.ProgressChanged -= WB_ProgressChanged;
            webBrowser.Navigating -= WebBrowser_Navigating;
            webBrowser.NewWindow -= WebBrowser_NewWindow;
            webBrowser.DocumentCompleted -= WebBrowser_DocumentCompleted;
            webBrowser.Navigated -= WebBrowser_Navigated;
            LeaveModeInternal(webBrowser);
            this.IsActive = false;
        }

        protected virtual void OnNewWindow(CancelEventArgs e) { }
        protected virtual void OnNavigating(WebBrowserNavigatingEventArgs e) { }
        protected virtual void OnNavigated(string url) { }
        protected virtual void OnDocumentCompleted(string html, string url)
        {

        }

        private void OnProgressChanged(int percentage)
        {
            WB.OnProgressChanged(this, new ProgressChangedEventArgs(percentage, null));
        }

        public async Task<HtmlElement> SynchronousLoadDocument(string url, string endUrl, int timeoutSeconds, CancellationToken token)
        {
            lock (syncObj)
            {
                IsSynchronousNavigating = true;
            }
            this.SyncNavContext = new SynchronousNavigationContext
            {
                StartUrl = url,
                EndUrl = endUrl,
                Tcs = new TaskCompletionSource<SynchronousLoadResult>(),
                TimeoutSeconds = timeoutSeconds
            };
            using (token.Register(() => SyncNavContext.Tcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                this.WB.Navigate(url);
                if ((await SyncNavContext.Tcs.Task).Success) // wait for DocumentCompleted
                {
                    return WB.Document.Body;
                }
            }
            return null;
        }
        public async Task<string> SynchronousLoadString(string url, CancellationToken token)
        {
            lock (syncObj)
            {
                IsSynchronousNavigating = true;
            }
            this.SyncNavContext = new SynchronousNavigationContext
            {
                StartUrl = url,
                EndUrl = url,
                Tcs = new TaskCompletionSource<SynchronousLoadResult>(),
                TimeoutSeconds = 5
            };
            using (token.Register(() => SyncNavContext.Tcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                this.WB.Navigate(url);
                if ((await SyncNavContext.Tcs.Task).Success) // wait for DocumentCompleted
                {
                    return WB.DocumentText;
                }
            }
            return string.Empty;
        }

        public void Navigate(string url)
        {
            this.WB.Navigate(url);
        }

        public T Active()
        {
            WB.TransactionToNext(this);
            return (T)WB.CurrentMode;
        }
    }

    public interface IWebBrowserMode
    {
        void Enter(ExtendedWinFormsWebBrowser extendedWinFormsWebBrowser);
        void Leave(ExtendedWinFormsWebBrowser extendedWinFormsWebBrowser);
        Task<HtmlElement> SynchronousLoadDocument(string url, string endUrl, int timeoutSeconds, CancellationToken token);
        Task<string> SynchronousLoadString(string url, CancellationToken token);
    }

    public interface IModeActivator<T>
    {
        T Active();
    }
}
