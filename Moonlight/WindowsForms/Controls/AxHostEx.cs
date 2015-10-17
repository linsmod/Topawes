using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Moonlight.WindowsForms.Controls
{
    public delegate void FileDownloadEventHandler(object sender, FileDownloadEventArgs e);
    public class FileDownloadEventArgs
    {
        public FileDownloadEventArgs(bool load, bool cancel)
        {
            this.cancel = cancel;
            this.load = load;
        }

        protected bool cancel;
        protected bool load;

        public bool Load
        {
            get { return load; }
        }

        public bool Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }
    }
    public class AxHostEx : AxHost, IMyWebBrowserEvents
    {
        protected string url;
        private IMyWebBrowser control;
        private ConnectionPointCookie cookie;
        public event FileDownloadEventHandler FileDownload;
        public AxHostEx() : base("8856f961-340a-11d0-a96b-00c04fd705a2")
        {

        }
        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                object o = null;
                control.Navigate(url, ref o, ref o, ref o, ref o);
            }
        }
        protected override void AttachInterfaces()
        {
            try { control = (IMyWebBrowser)GetOcx(); }
            catch { }
        }
        protected override void CreateSink()
        {
            try
            {
                cookie = new ConnectionPointCookie(control, this, typeof(IMyWebBrowserEvents));
            }
            catch { }
        }

        protected override void DetachSink()
        {
            try
            {
                cookie.Disconnect();
            }
            catch { }
        }
        public void RaiseFileDownload(bool load, ref bool cancel)
        {
            FileDownloadEventArgs e = new FileDownloadEventArgs(load, cancel);
            if (FileDownload != null)
                FileDownload(this, e);
            cancel = e.Cancel;
        }
    }
    [Guid("eab22ac1-30c1-11cf-a7eb-0000c05bae0b")]
    interface IMyWebBrowser
    {
        void GoBack();
        void GoForward();
        void GoHome();
        void GoSearch();
        void Navigate(string url, ref object flags, ref object targetFrame, ref
            object postData, ref object headers);
        void Refresh();
        void Refresh2();
        void Stop();
        void GetApplication();
        void GetParent();
        void GetContainer();
    }

    [Guid("34A715A0-6587-11D0-924A-0020AFC7AC4D"),
    InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IMyWebBrowserEvents
    {
        [DispId(270)]
        void RaiseFileDownload(bool Load, ref bool Cancel);
    }
}
