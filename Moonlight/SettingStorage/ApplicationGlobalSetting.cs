using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moonlight.SettingStorage
{
    public class ApplicationGlobalSetting
    {
        /// <summary>
        /// id is setting key
        /// </summary>
        public string Id { get; set; }
        public string Value { get; set; }
        public string TypeName { get; set; }
        public string Group { get; set; }
        public bool Protected { get; set; }
    }
}
