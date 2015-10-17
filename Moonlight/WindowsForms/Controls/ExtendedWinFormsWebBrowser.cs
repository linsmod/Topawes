using Microsoft.ShDocVw;
using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using CSWebDownloader;

namespace Moonlight.WindowsForms.Controls
{
    public class ExtendedWinFormsWebBrowser : System.Windows.Forms.WebBrowser
    {
        private AxHost.ConnectionPointCookie cookie;
        private ExtendedWinFormsWebBrowserEventHelper helper;
        public event EventHandler<EventArgs> NavigationError;
        public event ProgressChangedEventHandler ProgressChanged2;
        public event EventHandler<HttpDownloadProgressChangedEventArgs> DownloadProgressChanged;
        public event EventHandler<HttpDownloadCompletedEventArgs> DownloadCompleted;
        public event EventHandler DownloadStatusChanged;
        public const int SET_FEATURE_ON_PROCESS = 2;
        public ExtendedWinFormsWebBrowser()
        {
            NativeMethods.CoInternetSetFeatureEnabled((int)INTERNETFEATURELIST.FEATURE_MIME_HANDLING, (uint)SET_FEATURE_ON_PROCESS, true);
            NativeMethods.CoInternetSetFeatureEnabled((int)INTERNETFEATURELIST.FEATURE_MIME_SNIFFING, (uint)SET_FEATURE_ON_PROCESS, true);
        }

        protected override WebBrowserSiteBase CreateWebBrowserSiteBase()
        {
            return new ExtendedWebBrowserSite(this);
        }

        protected class ExtendedWebBrowserSite : WebBrowserSite, IServiceProvider
        {
            IEDownloadManager _manager;
            ExtendedWinFormsWebBrowser host;
            public ExtendedWebBrowserSite(ExtendedWinFormsWebBrowser host) : base(host)
            {
                this.host = host;
                _manager = new IEDownloadManager(GetHostCookie);
                _manager.DownloadCompleted += host.DownloadCompleted;
                _manager.DownloadProgressChanged += host.DownloadProgressChanged;
                _manager.StatusChanged += host.DownloadStatusChanged;
            }
            Guid serviceId = new Guid("bdb9c34c-d0ca-448e-b497-8de62e709744");
            Guid interfaceId = new Guid("988934A4-064B-11D3-BB80-00104B35E7F9");
            public int QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject)
            {
                if ((riid == interfaceId))
                {
                    ppvObject = Marshal.GetComInterfaceForObject(_manager, typeof(IDownloadManager));
                    return 0;
                }
                ppvObject = IntPtr.Zero;
                return -1;
            }

            private string GetHostCookie(string url)
            {
                return host.Document.Cookie + ";" + IECookieHelper.GetGlobalCookies(url);
            }
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

        internal void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var h = ProgressChanged2;
            if (h != null)
            {
                h(this, e);
            }
        }
    }
}
