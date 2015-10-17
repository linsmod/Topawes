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
    public partial class MoonUpDown : NumericUpDown
    {
        public string UserSettingId { get; set; }
        public event EventHandler<AppUserSettingChangedEventArgs> AppUserSettingChanged;
        public MoonUpDown()
        {
            InitializeComponent();
        }
        public void SetupAfterUserSettingInitialized(string name)
        {
            this.UserSettingId = name;
            this.Value = AppSetting.UserSetting.Get(this.UserSettingId, this.Value);
            //用户可能还没有配置，用默认值写一个新的
            AppSetting.UserSetting.Set(this.UserSettingId, this.Value);
            //侦听变更
            this.ValueChanged += MoonUpDown_ValueChanged;
        }

        private void MoonUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (this.DecimalPlaces == 0)
            {
                this.Value = (int)Value;
            }
            else if (this.DecimalPlaces > 0)
            {
                this.Value = decimal.Parse(Value.ToString("f" + this.DecimalPlaces));
            }
            var old = AppSetting.UserSetting.Get<decimal>(this.UserSettingId);
            if (this.Value != old)
            {
                AppSetting.UserSetting.Set(this.UserSettingId, this.Value, AppUserSettingProtect);
                this.OnAppUserSettingChanged(old, this.Value);
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
