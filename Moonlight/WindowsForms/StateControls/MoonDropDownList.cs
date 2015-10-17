using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Moonlight.WindowsForms.StateControls
{
    public partial class MoonDropDownList : ComboBox
    {
        public string AppUserSettingId { get; set; }
        public MoonDropDownList()
        {
            InitializeComponent();
        }

        public void SetupAfterUserSettingInitialized(string settingId, string[] values, string value, bool protect = false)
        {
            this.AppUserSettingId = settingId;
            this.DataSource = values;
            this.SelectedItem = value;
            //获取控件默认值或用户配置的值
            this.SelectedItem = AppSetting.UserSetting.Get(this.AppUserSettingId, this.SelectedItem);
            //用户可能还没有配置，用默认值写一个新的
            AppSetting.UserSetting.Set(this.AppUserSettingId, this.SelectedItem);
            //侦听变更
            this.SelectionChangeCommitted += MoonDropDownList_SelectionChangeCommitted;
        }

        private void MoonDropDownList_SelectionChangeCommitted(object sender, EventArgs e)
        {
            AppSetting.UserSetting.Set(this.AppUserSettingId, this.SelectedItem);
        }

        public bool AppUserSettingProtect { get; set; }
    }
}
