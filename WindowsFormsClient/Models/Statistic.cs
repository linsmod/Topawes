using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsClient.Models
{
    public class Statistic
    {

        public string Id { get; set; }

        /// <summary>
        /// 付款
        /// </summary>
        public int PayCount { get; set; }
        /// <summary>
        /// 下单
        /// </summary>
        public int BuyCount { get; set; }

        /// <summary>
        /// 拦截成功
        /// </summary>
        public int InterceptSuccess { get; set; }

        /// <summary>
        /// 拦截失败
        /// </summary>
        public int InterceptFailed { get; set; }
    }
}
