using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsClient.BizEventArgs
{
    public class AccountBizEventArgs : EventArgs
    {
        public static AccountBizEventArgs FromHtml(string html)
        {
            return new AccountBizEventArgs { };
        }
    }
}
