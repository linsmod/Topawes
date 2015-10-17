using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsClient.BizEventArgs
{
    public class StockBizEventArgs : EventArgs
    {
        public static StockBizEventArgs FromHtml(string html)
        {
            return new StockBizEventArgs { };
        }
    }
}
