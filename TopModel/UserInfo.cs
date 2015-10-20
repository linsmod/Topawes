using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TopModel
{
    public class UserInfo
    {
        public string UserName { get; set; }
        public DateTime? LicenseExpires { get; set; }
        public bool TopManagerInitialized { get; set; }
    }
}
