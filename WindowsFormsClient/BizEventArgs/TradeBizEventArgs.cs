using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsClient.BizEventArgs
{
    public class TradeBizEventArgs : EventArgs
    {
        public static TradeBizEventArgs FromHtml(string html)
        {
            return new TradeBizEventArgs { };
        }
    }
}
