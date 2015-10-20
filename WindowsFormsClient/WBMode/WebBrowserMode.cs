
using Moonlight.WindowsForms.Controls;
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
    public abstract class WebBrowserMode<T> : IWebBrowserMode where T : IWebBrowserMode
    {
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

        public void Navigate(string url)
        {
            this.WB.Navigate(url);
        }
    }

    public interface IWebBrowserMode
    {
        void Enter(ExtendedWinFormsWebBrowser extendedWinFormsWebBrowser);
        void Leave(ExtendedWinFormsWebBrowser extendedWinFormsWebBrowser);
    }
}
