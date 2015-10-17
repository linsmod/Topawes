using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsClient.BizEventArgs
{
    public class IndexBizEventArgs : EventArgs
    {
        public static IndexBizEventArgs FromHtml(string html)
        {
            return new IndexBizEventArgs { };
        }
    }
}
