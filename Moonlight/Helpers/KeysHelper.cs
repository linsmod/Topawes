using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Moonlight.Helpers
{
    public class KeysHelper
    {
        public static bool IsControlKeyPressedDown
        {
            get
            {
                // CTRL is pressed
                return (Control.ModifierKeys & Keys.Control) == Keys.Control;
            }
        }
    }
}
