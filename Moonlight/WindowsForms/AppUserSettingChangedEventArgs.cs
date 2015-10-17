using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moonlight.WindowsForms
{
    public class AppUserSettingChangedEventArgs : EventArgs
    {
        public string UserSettingId { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}
