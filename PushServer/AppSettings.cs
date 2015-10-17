using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushServer
{
    public class AppSettings
    {
        /// <summary>
        /// 首次添加的用户自动授权
        /// </summary>
        public static bool AutoAddSoftwareLicenseWhenAddUser = true;

        /// <summary>
        /// 自动授权天数
        /// </summary>
        public static int AutoLicenseDays = 30;
    }
}
