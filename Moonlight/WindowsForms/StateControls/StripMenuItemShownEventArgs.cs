using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moonlight.WindowsForms.StateControls
{
    public class StripMenuItemShownOnContextEventArgs : EventArgs
    {
        public IEnumerable TargetObjects { get; private set; }
        public StripMenuItemShownOnContextEventArgs(IEnumerable TargetObjects)
        {
            this.TargetObjects = TargetObjects;
        }
        /// <summary>
        /// 是否显示
        /// </summary>
        public bool? Available { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? Enabled { get; set; }
    }
}
