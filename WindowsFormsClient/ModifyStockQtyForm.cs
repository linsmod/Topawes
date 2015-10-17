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
using TopModel;
using WinFormsClient.Models;

namespace WinFormsClient
{
    public partial class ModifyStockQtyForm : BaseForm
    {
        public IEnumerable<ProductItem> productList;
        public ModifyStockQtyForm()
        {
            InitializeComponent();
            this.Text = "修改库存";
            QQ直充库存.SetupAfterUserSettingInitialized("QQ直充库存");
            点卡直充库存.SetupAfterUserSettingInitialized("点卡直充库存");
            话费直充库存.SetupAfterUserSettingInitialized("话费直充库存");

            QQ直充库存.SetupAfterUserSettingInitialized("修改QQ直充库存");
            点卡直充库存.SetupAfterUserSettingInitialized("修改点卡直充库存");
            话费直充库存.SetupAfterUserSettingInitialized("修改话费直充库存");

            修改QQ直充库存.AppUserSettingChanged += 修改QQ直充库存_AppUserSettingChanged;
            修改点卡直充库存.AppUserSettingChanged += 修改点卡直充库存_AppUserSettingChanged;
            修改话费直充库存.AppUserSettingChanged += 修改话费直充库存_AppUserSettingChanged;

            QQ直充库存.Enabled = AppSetting.UserSetting.Get<bool>("修改QQ直充库存", 修改QQ直充库存.Checked);
            点卡直充库存.Enabled = AppSetting.UserSetting.Get<bool>("修改点卡直充库存", 修改点卡直充库存.Checked);
            话费直充库存.Enabled = AppSetting.UserSetting.Get<bool>("修改话费直充库存", 修改话费直充库存.Checked);
        }

        private void 修改话费直充库存_AppUserSettingChanged(object sender, Moonlight.WindowsForms.AppUserSettingChangedEventArgs e)
        {
            话费直充库存.Enabled = (bool)e.NewValue;
        }

        private void 修改点卡直充库存_AppUserSettingChanged(object sender, Moonlight.WindowsForms.AppUserSettingChangedEventArgs e)
        {
            点卡直充库存.Enabled = (bool)e.NewValue;
        }

        private void 修改QQ直充库存_AppUserSettingChanged(object sender, Moonlight.WindowsForms.AppUserSettingChangedEventArgs e)
        {
            QQ直充库存.Enabled = (bool)e.NewValue;
        }

        public ModifyStockQtyForm(IEnumerable<ProductItem> productList) : this()
        {
            this.productList = productList;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            foreach (Control item in this.Controls)
            {
                item.Enabled = false;
            }
            await (this.Owner as WinFormsClient).ModifyStock((int)this.QQ直充库存.Value, productList.Where(x => x.ItemSubName == "QQ直充"));
            await (this.Owner as WinFormsClient).ModifyStock((int)this.点卡直充库存.Value, productList.Where(x => x.ItemSubName == "点卡直充"));
            await (this.Owner as WinFormsClient).ModifyStock((int)this.话费直充库存.Value, productList.Where(x => x.ItemSubName == "话费直充"));
            this.Close();
        }
    }
}
