using Moonlight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsClient
{
    public partial class WhiteListForm : BaseForm
    {
        public WhiteListForm(string title)
        {
            InitializeComponent();
            this.Text = title;
            this.textBoxBlackList.Lines = AppSetting.UserSetting.Get<string[]>("买家白名单", new string[0]);
            if (Owner != null) Owner.Enabled = false;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            AppSetting.UserSetting.Set<string[]>("买家白名单", textBoxBlackList.Lines);
            if (Owner != null) Owner.Enabled = true;
            this.Hide();
        }
    }
}
