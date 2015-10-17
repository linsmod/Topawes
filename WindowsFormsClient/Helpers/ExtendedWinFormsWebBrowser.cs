using Microsoft.ShDocVw;
using System;
using System.Windows.Forms;
using WinFormsClient.WBMode;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Phone.Tools
{
    public class ExtendedWinFormsWebBrowser : System.Windows.Forms.WebBrowser
    {
        private AxHost.ConnectionPointCookie cookie;
        private ExtendedWinFormsWebBrowserEventHelper helper;
        public event EventHandler<EventArgs> NavigationError;
        public event ProgressChangedEventHandler ProgressChanged2;

        public IWebBrowserMode PreviousMode { get; set; }
        public IWebBrowserMode CurrentMode { get; set; }
        public ExtendedWinFormsWebBrowser()
        {
        }
        public ExtendedWinFormsWebBrowser(IWebBrowserMode workMode)
        {
            TransactionToNext(workMode);
        }



        protected override void CreateSink()
        {
            base.CreateSink();
            this.helper = new ExtendedWinFormsWebBrowserEventHelper(this);
            this.cookie = new AxHost.ConnectionPointCookie(base.ActiveXInstance, this.helper, typeof(DWebBrowserEvents2));
        }
        protected override void DetachSink()
        {
            if (this.cookie != null)
            {
                this.cookie.Disconnect();
                this.cookie = null;
            }
            base.DetachSink();
        }
        internal void NavigateError(string url, int statusCode)
        {
            EventHandler<EventArgs> navigationError = this.NavigationError;
            if (navigationError != null)
            {
                navigationError(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 进入另一个模式
        /// </summary>
        /// <param name="next"></param>
        public void TransactionToNext(IWebBrowserMode next)
        {
            if (CurrentMode != null)
            {
                if (CurrentMode == next)
                {
                    return;
                }

                CurrentMode.Leave(this);
            }
            next.Enter(this);
            this.PreviousMode = CurrentMode;
            this.CurrentMode = next;
        }

        /// <summary>
        /// 转到上一个模式
        /// </summary>
        public void TransactionToPrev()
        {
            if (this.PreviousMode != null)
            {
                this.TransactionToNext(this.PreviousMode);
            }
            else
            {
                throw new Exception("不存在上一个模式");
            }
        }

        internal void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var h = ProgressChanged2;
            if (h != null)
            {
                h(this, e);
            }
        }

        public async Task<HtmlElement> SynchronousLoad(string url)
        {
            return await this.CurrentMode.SynchronousLoadDocument(url, url, 15, CancellationToken.None);
        }

        public async Task<HtmlElement> SynchronousLoad(string url, string endUrl, int timeoutInSeconds)
        {
            return await this.CurrentMode.SynchronousLoadDocument(url, endUrl, timeoutInSeconds, CancellationToken.None);
        }

        public async Task<string> SynchronousLoadString(string url)
        {
            return await this.CurrentMode.SynchronousLoadString(url, CancellationToken.None);
        }

        
    }
}
