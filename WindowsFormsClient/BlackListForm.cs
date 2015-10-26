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
    public partial class BlackListForm : BaseForm
    {
        public BlackListForm(string title)
        {
            InitializeComponent();
            this.Text = title;
            this.textBoxBlackList.Lines = AppSetting.UserSetting.Get("买家黑名单", new string[0]);
            if (Owner != null) Owner.Enabled = false;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            AppSetting.UserSetting.Set("买家黑名单", textBoxBlackList.Lines);
            if (Owner != null) Owner.Enabled = true;
            this.Hide();
        }

        [DefaultValue("")]
        public static string BlackList;
    }
}
