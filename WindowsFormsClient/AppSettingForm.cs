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
    public partial class AppSettingForm : BaseForm
    {
        public AppSettingForm()
        {
            InitializeComponent();
            this.textBoxUserName.Text = AppSetting.UserSetting.Get<string>("TaoUserName");
            this.textBoxPassword.Text = AppSetting.UserSetting.Get<string>("TaoPassword");
            this.checkBoxAutoSubmitLogin.Checked = AppSetting.UserSetting.Get<bool>("AutoSelectLoginedAccount");
        }


        private void buttonSave_Click(object sender, EventArgs e)
        {
            AppSetting.UserSetting.Set<bool>("AutoSelectLoginedAccount", checkBoxAutoSubmitLogin.Checked);
            AppSetting.UserSetting.Set("TaoPassword", this.textBoxPassword.Text, true);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonClean_Click(object sender, EventArgs e)
        {
            AppSetting.UserSetting.Set("TaoPassword", "");
            AppSetting.UserSetting.Set("TaoUserName", "");
            AppSetting.UserSetting.SetNull("AutoSelectLoginedAccount");
            MessageBox.Show("账号信息已清除，可使用新的账号登陆。");
            this.Close();
        }
    }
}
