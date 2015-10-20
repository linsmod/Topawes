using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32;
using CSWebDownloader;
using System.Linq;
using Moonlight.WindowsForms.Controls;

namespace Moonlight.WindowsForms
{
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Runtime.InteropServices.Guid("bdb9c34c-d0ca-448e-b497-8de62e709744")]
    public class IEDownloadManager : IDownloadManager
    {
        public event EventHandler StatusChanged;
        ExtendedWinFormsWebBrowser wb;
        HttpDownloadClient downloadClient;
        Func<string, string> GetCookie;

        public IEDownloadManager(Func<string, string> getCookie, ExtendedWinFormsWebBrowser wb)
        {
            this.GetCookie = getCookie;
            this.wb = wb;
        }

        /// <summary>
        /// Return S_OK (0) so that IE will stop to download the file itself. 
        /// Else the default download user interface is used.
        /// </summary>
        public int Download(IMoniker pmk, IBindCtx pbc, uint dwBindVerb, int grfBINDF,
            IntPtr pBindInfo, string pszHeaders, string pszRedir, uint uiCP)
        {
            // Get the display name of the pointer to an IMoniker interface that specifies the object to be downloaded.
            string name;
            pmk.GetDisplayName(pbc, null, out name);
            if (!string.IsNullOrEmpty(name))
            {
                Uri url;
                if (Uri.TryCreate(name, UriKind.Absolute, out url))
                {
                    Debug.WriteLine("DownloadManager: initial URL is: " + url);
                    NativeMethods.CreateBindCtx(0, out pbc);

                    downloadClient = new HttpDownloadClient(url.AbsoluteUri);
                    downloadClient.DownloadCompleted += (s,e)=> { if (wb.DownloadCompleted != null) wb.DownloadCompleted(s, e); } ;
                    downloadClient.DownloadProgressChanged += (s, e) => { if (wb.DownloadProgressChanged != null) wb.DownloadProgressChanged(s, e); };
                    downloadClient.StatusChanged += (s, e) => { if (wb.DownloadStatusChanged != null) wb.DownloadStatusChanged(s, e); };
                    downloadClient.OverWriteExsitFile = true;
                    if (this.GetCookie != null)
                    {
                        downloadClient.Cookie = this.GetCookie(url.AbsoluteUri);
                    }
                    string filename = string.Empty;
                    downloadClient.CheckUrl(out filename);
                    if (downloadClient.Status == HttpDownloadClientStatus.Completed)
                        return 0;
                    //if (string.IsNullOrEmpty(filename))
                    //{
                    //    downloadClient.DownloadPath = string.Format("{0}\\{1}",
                    //        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    //        downloadClient.Url.Segments.Last());
                    //}
                    //else
                    //{
                    //    downloadClient.DownloadPath = string.Format("{0}\\{1}",
                    //        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    //        filename);
                    //}
                    downloadClient.DownloadPath = Path.GetTempFileName();

                    downloadClient.Start();
                    //RegisterCallback(pbc, url);
                    //BindMonikerToStream(pmk, pbc);
                    return 0;
                }
            }
            return 1;
        }
    }
}
