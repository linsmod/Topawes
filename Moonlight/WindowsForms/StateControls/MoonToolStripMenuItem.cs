using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace Moonlight.WindowsForms.StateControls
{
    public partial class MoonToolStripMenuItem : ToolStripMenuItem
    {
        public StateItemControlOption ControlOptions = new StateItemControlOption();
        public event EventHandler<StripMenuItemShownOnContextEventArgs> Showing;
        public event EventHandler<AppUserSettingChangedEventArgs> AppUserSettingChanged;
        public MoonToolStripMenuItem()
        {
            InitializeComponent();
        }
        public MoonToolStripMenuItem(Image image) : base(image) { }
        //
        // 摘要:
        //     初始化显示指定文本的 System.Windows.Forms.ToolStripMenuItem 类的新实例。
        //
        // 参数:
        //   text:
        //     要在菜单项上显示的文本。
        public MoonToolStripMenuItem(string text) : base(text) { }
        //
        // 摘要:
        //     初始化显示指定文本和图像的 System.Windows.Forms.ToolStripMenuItem 类的新实例。
        //
        // 参数:
        //   text:
        //     要在菜单项上显示的文本。
        //
        //   image:
        //     要在控件上显示的 System.Drawing.Image。
        public MoonToolStripMenuItem(string text, Image image) : base(text, image) { }
        //
        // 摘要:
        //     初始化显示指定文本和图像及包含指定 System.Windows.Forms.ToolStripItem 集合的 System.Windows.Forms.ToolStripMenuItem
        //     类的新实例。
        //
        // 参数:
        //   text:
        //     要在菜单项上显示的文本。
        //
        //   image:
        //     要在控件上显示的 System.Drawing.Image。
        //
        //   dropDownItems:
        //     单击该控件时显示的菜单项。
        public MoonToolStripMenuItem(string text, Image image, params ToolStripItem[] dropDownItems) : base(text, image, dropDownItems) { }
        //
        // 摘要:
        //     初始化 System.Windows.Forms.ToolStripMenuItem 类的新实例，该类显示指定的文本和图像，并在单击 System.Windows.Forms.ToolStripMenuItem
        //     时执行指定的操作。
        //
        // 参数:
        //   text:
        //     要在菜单项上显示的文本。
        //
        //   image:
        //     要在控件上显示的 System.Drawing.Image。
        //
        //   onClick:
        //     单击该控件时引发 System.Windows.Forms.Control.Click 事件的事件处理程序。
        public MoonToolStripMenuItem(string text, Image image, EventHandler onClick) : base(text, image, onClick) { }
        //
        // 摘要:
        //     初始化 System.Windows.Forms.ToolStripMenuItem 类的新实例，该类显示指定的文本和图像，在单击 System.Windows.Forms.ToolStripMenuItem
        //     时执行指定的操作，并显示指定的快捷键。
        //
        // 参数:
        //   text:
        //     要在菜单项上显示的文本。
        //
        //   image:
        //     要在控件上显示的 System.Drawing.Image。
        //
        //   onClick:
        //     单击该控件时引发 System.Windows.Forms.Control.Click 事件的事件处理程序。
        //
        //   shortcutKeys:
        //     System.Windows.Forms.Keys 的值之一，表示 System.Windows.Forms.ToolStripMenuItem 的快捷键。
        public MoonToolStripMenuItem(string text, Image image, EventHandler onClick, Keys shortcutKeys) : base(text, image, onClick, shortcutKeys) { }
        //
        // 摘要:
        //     用指定名称初始化 System.Windows.Forms.ToolStripMenuItem 类的新实例，该类显示指定的文本和图像，并在单击 System.Windows.Forms.ToolStripMenuItem
        //     时执行指定的操作。
        //
        // 参数:
        //   text:
        //     要在菜单项上显示的文本。
        //
        //   image:
        //     要在控件上显示的 System.Drawing.Image。
        //
        //   onClick:
        //     单击该控件时引发 System.Windows.Forms.Control.Click 事件的事件处理程序。
        //
        //   name:
        //     菜单项的名称。
        public MoonToolStripMenuItem(string text, Image image, EventHandler onClick, string name) : base(text, image, onClick, name) { }
        public void SetupAfterUserSettingInitialized(string name)
        {
            this.Name = name;
            //获取控件默认值或用户配置的值
            this.Checked = AppSetting.UserSetting.Get(this.Name, this.Checked);
            //用户可能还没有配置，用默认值写一个新的
            AppSetting.UserSetting.Set(this.Name, this.Checked);
            //侦听变更
            this.CheckedChanged += MoonCheckBox_CheckedChanged;
        }

        private void MoonCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var old = AppSetting.UserSetting.Get<bool>(this.Name);
            if (this.Checked != old)
            {
                AppSetting.UserSetting.Set(this.Name, this.Checked, AppUserSettingProtect);
                this.OnAppUserSettingChanged(old, this.Checked);
            }
        }

        public void OnAppUserSettingChanged(object oldValue, object newValue)
        {
            if (AppUserSettingChanged != null)
            {
                var h = AppUserSettingChanged;
                h(this, new AppUserSettingChangedEventArgs { UserSettingId = this.Name, OldValue = oldValue, NewValue = newValue });
            }
        }

        public bool AppUserSettingProtect { get; set; }

        private bool EnabledOrg;
        private bool AvailableOrg;
        public void OnContextMenuStripClosing()
        {
            //restore original state
            this.Enabled = EnabledOrg;
            this.Available = AvailableOrg;
        }
        /// <summary>
        /// 触发一个ShownOnContext事件，让订阅ShownOnContext事件的用户决定此控件的表现方式
        /// </summary>
        /// <param name="targets"></param>
        public void OnContextMenuStripOpening(IEnumerable targets)
        {
            //backup original state
            this.EnabledOrg = Enabled;
            this.AvailableOrg = Available;
            ApplyControlOption(targets);
            var h = Showing;
            if (h != null)
            {
                var eventArgs = new StripMenuItemShownOnContextEventArgs(targets);
                h(this, eventArgs);
                if (eventArgs.Available.HasValue)
                {
                    this.Available = eventArgs.Available.Value;
                }
                else
                {
                    this.Available = true;
                }
                if (eventArgs.Enabled.HasValue)
                {
                    Enabled = eventArgs.Enabled.Value;
                }
                else
                {
                    Enabled = true;
                }
            }
        }

        private void ApplyControlOption(IEnumerable targets)
        {
            var items = targets.AsList<object>();
            if (!items.Any())
            {
                SetEnableControlValue(ControlOptions.WhenNone.Enabled);
                SetAvailableControlValue(ControlOptions.WhenNone.Avaiable);
            }
            else
            {
                if (items.Count == 1)
                {
                    SetEnableControlValue(ControlOptions.WhenSingle.Enabled);
                    SetAvailableControlValue(ControlOptions.WhenSingle.Avaiable);
                }
                else
                {
                    SetEnableControlValue(ControlOptions.WhenMulti.Enabled);
                    SetAvailableControlValue(ControlOptions.WhenMulti.Avaiable);
                }
                SetEnableControlValue(ControlOptions.WhenAny.Enabled);
                SetAvailableControlValue(ControlOptions.WhenAny.Avaiable);
            }
        }

        private void SetEnableControlValue(bool? input)
        {
            if (input.HasValue)
            {
                this.Enabled = input.Value;
            }
        }

        private void SetAvailableControlValue(bool? input)
        {
            if (input.HasValue)
            {
                this.Available = input.Value;
            }
        }

    }

    public class StateItemControlOption
    {
        internal StateItemControlOption() { }

        /// <summary>
        /// 单个元素的时候
        /// </summary>
        public SceneControl WhenSingle = new SceneControl { };
        /// <summary>
        /// 多个元素的时候
        /// </summary>
        public SceneControl WhenMulti = new SceneControl { };
        /// <summary>
        /// 没有元素的时候
        /// </summary>
        public SceneControl WhenNone = new SceneControl { };

        /// <summary>
        /// 当有任何元素的时候，比WhenNone和WhenMulti权重高
        /// </summary>
        public SceneControl WhenAny = new SceneControl { };

        public class SceneControl
        {
            public bool? Enabled { get; set; }
            public bool? Avaiable { get; set; }
        }
    }
}
