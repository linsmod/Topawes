using System;
using System.Windows.Forms;

namespace Moonlight.WindowsForms.StateControls
{
    public partial class MoonCheckBox : CheckBox
    {
        public string UserSettingId { get; set; }
        public event EventHandler<AppUserSettingChangedEventArgs> AppUserSettingChanged;
        public MoonCheckBox()
        {
            InitializeComponent();
        }

        public void SetupAfterUserSettingInitialized(string settingId)
        {
            this.UserSettingId = settingId;
            //获取控件默认值或用户配置的值
            this.Checked = AppSetting.UserSetting.Get(this.UserSettingId, this.Checked);
            //用户可能还没有配置，用默认值写一个新的
            AppSetting.UserSetting.Set(this.UserSettingId, this.Checked);
            //侦听变更
            this.CheckedChanged += MoonCheckBox_CheckedChanged;
        }

        private void MoonCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var old = AppSetting.UserSetting.Get<bool>(this.UserSettingId);
            if (this.Checked != old)
            {
                AppSetting.UserSetting.Set(this.UserSettingId, this.Checked, AppUserSettingProtect);
                this.OnAppUserSettingChanged(old, this.Checked);
            }
        }

        public void OnAppUserSettingChanged(object oldValue, object newValue)
        {
            if (AppUserSettingChanged != null)
            {
                var h = AppUserSettingChanged;
                h(this, new AppUserSettingChangedEventArgs { UserSettingId = this.UserSettingId, OldValue = oldValue, NewValue = newValue });
            }
        }

        public bool AppUserSettingProtect { get; set; }
    }
}
