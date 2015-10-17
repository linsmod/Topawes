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
using WinFormsClient.Helpers;

namespace WinFormsClient
{

    public partial class SetProfitForm : BaseForm
    {
        public List<long> ProductIds { get; set; }
        public SetProfitForm(List<long> productIds) : this()
        {
            ProductIds = productIds;
        }
        public SetProfitForm()
        {
            InitializeComponent();
            this.Text = "更新商品利润";
            QQ直充利润UpDown.SetupAfterUserSettingInitialized("QQ直充利润");
            点卡直充利润UpDown.SetupAfterUserSettingInitialized("点卡直充利润");
            话费直充利润UpDown.SetupAfterUserSettingInitialized("话费直充利润");

            QQ直充利润CheckBox.SetupAfterUserSettingInitialized("处理QQ直充利润");
            点卡直充利润CheckBox.SetupAfterUserSettingInitialized("处理点卡直充利润");
            话费直充利润CheckBox.SetupAfterUserSettingInitialized("处理话费直充利润");

            QQ直充利润CheckBox.AppUserSettingChanged += QQ直充利润CheckBox_AppUserSettingChanged;
            点卡直充利润CheckBox.AppUserSettingChanged += 点卡直充利润CheckBox_AppUserSettingChanged;
            话费直充利润CheckBox.AppUserSettingChanged += 话费直充利润CheckBox_AppUserSettingChanged;

            QQ直充利润UpDown.Enabled = AppSetting.UserSetting.Get<bool>("处理QQ直充利润", QQ直充利润CheckBox.Checked);
            点卡直充利润UpDown.Enabled = AppSetting.UserSetting.Get<bool>("处理点卡直充利润", 点卡直充利润CheckBox.Checked);
            话费直充利润UpDown.Enabled = AppSetting.UserSetting.Get<bool>("处理话费直充利润", 话费直充利润CheckBox.Checked);
        }

        private void 话费直充利润CheckBox_AppUserSettingChanged(object sender, Moonlight.WindowsForms.AppUserSettingChangedEventArgs e)
        {
            话费直充利润UpDown.Enabled = (bool)e.NewValue;
        }

        private void 点卡直充利润CheckBox_AppUserSettingChanged(object sender, Moonlight.WindowsForms.AppUserSettingChangedEventArgs e)
        {
            点卡直充利润UpDown.Enabled = (bool)e.NewValue;
        }

        private void QQ直充利润CheckBox_AppUserSettingChanged(object sender, Moonlight.WindowsForms.AppUserSettingChangedEventArgs e)
        {
            QQ直充利润UpDown.Enabled = (bool)e.NewValue;
        }

        private async void 确定button_Click(object sender, EventArgs e)
        {
            var list = AppDatabase.db.ProductItems.FindAll().Where(x => ProductIds.Contains(x.Id));
            await (Task)(Owner as WinFormsClient).Invoke(new SetProfitBySubNameDelegate((Owner as WinFormsClient).SetProfitBySubName), list.ToList());
            this.Close();
        }
    }
}
