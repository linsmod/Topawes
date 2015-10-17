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
        public StateItemShowingOption ShowingOption = StateItemShowingOption.EnableAlwasy | StateItemShowingOption.ShowAlwasy;
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

        /// <summary>
        /// 触发一个ShownOnContext事件，让订阅ShownOnContext事件的用户决定此控件的表现方式
        /// </summary>
        /// <param name="targets"></param>
        public void OnShownOnContext(IEnumerable targets)
        {
            this.Available = true;
            this.Enabled = true;
            CheckAvailableOption(targets);
            CheckShowingOption(targets);
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

        private void CheckAvailableOption(IEnumerable targets)
        {
            if (ShowingOption.HasFlag(StateItemShowingOption.ShowAlwasy))
            {
                this.Available = true;
            }
            else
            {
                var items = targets.AsList<object>();
                if (!items.Any())
                {
                    if (ShowingOption.HasFlag(StateItemShowingOption.HideIfNoneTarget))
                    {
                        this.Available = false;
                    }
                    else if (ShowingOption.HasFlag(StateItemShowingOption.ShowIfNoneTarget))
                    {
                        this.Available = true;
                    }
                }
                else
                {
                    if (ShowingOption.HasFlag(StateItemShowingOption.HideIfAnyTarget))
                    {
                        this.Available = false;
                    }
                    else if (ShowingOption.HasFlag(StateItemShowingOption.ShowIfAnyTarget))
                    {
                        this.Available = true;
                    }
                    else
                    {
                        if (items.Count == 1)
                        {
                            if (ShowingOption.HasFlag(StateItemShowingOption.HideIfSingleTarget))
                            {
                                this.Available = false;
                            }
                            else if (ShowingOption.HasFlag(StateItemShowingOption.ShowIfSingleTarget))
                            {
                                this.Available = true;
                            }
                        }
                        else
                        {
                            if (ShowingOption.HasFlag(StateItemShowingOption.HideIfMultiTargets))
                            {
                                this.Available = false;
                            }
                            else if (ShowingOption.HasFlag(StateItemShowingOption.ShowIfMultiTargets))
                            {
                                this.Available = true;
                            }
                        }
                    }
                }
            }
        }

        private void CheckShowingOption(IEnumerable targets)
        {
            if (ShowingOption.HasFlag(StateItemShowingOption.EnableAlwasy))
            {
                this.Enabled = true;
            }
            else
            {
                var items = targets.AsList<object>();
                if (!items.Any())
                {
                    if (ShowingOption.HasFlag(StateItemShowingOption.DisableIfNoneTarget))
                    {
                        this.Enabled = false;
                    }
                    else if (ShowingOption.HasFlag(StateItemShowingOption.EnableIfNoneTarget))
                    {
                        this.Enabled = true;
                    }
                }
                else
                {
                    if (ShowingOption.HasFlag(StateItemShowingOption.DisableIfAnyTarget))
                    {
                        this.Enabled = false;
                    }
                    else if (ShowingOption.HasFlag(StateItemShowingOption.EnableIfAnyTarget))
                    {
                        this.Enabled = true;
                    }
                    else
                    {
                        if (items.Count == 1)
                        {
                            if (ShowingOption.HasFlag(StateItemShowingOption.DisableIfSingleTarget))
                            {
                                this.Enabled = false;
                            }
                            else if (ShowingOption.HasFlag(StateItemShowingOption.EnableIfSingleTarget))
                            {
                                this.Enabled = true;
                            }
                        }
                        else
                        {
                            if (ShowingOption.HasFlag(StateItemShowingOption.DisableIfMultiTargets))
                            {
                                this.Enabled = false;
                            }
                            else if (ShowingOption.HasFlag(StateItemShowingOption.EnableIfMultiTargets))
                            {
                                this.Enabled = true;
                            }
                        }
                    }
                }
            }
        }
        
    }

    [Flags]
    public enum StateItemShowingOption
    {
        /// <summary>
        /// 没有目标的时候禁用
        /// </summary>
        DisableIfNoneTarget = 1,

        /// <summary>
        /// 没有目标的时候启用
        /// </summary>
        EnableIfNoneTarget = 2,
        /// <summary>
        /// 没有目标的时候不显示
        /// </summary>
        HideIfNoneTarget = 4,

        /// <summary>
        /// 没有目标的时候显示
        /// </summary>
        ShowIfNoneTarget = 8,

        /// <summary>
        /// 单个目标的时候禁用
        /// </summary>
        DisableIfSingleTarget = 16,

        /// <summary>
        /// 单个目标的时候启用
        /// </summary>
        EnableIfSingleTarget = 32,

        /// <summary>
        /// 单个目标的时候隐藏
        /// </summary>
        HideIfSingleTarget = 64,

        /// <summary>
        /// 单个目标时候显示
        /// </summary>
        ShowIfSingleTarget = 128,
        /// <summary>
        /// 多个目标的时候禁用
        /// </summary>
        DisableIfMultiTargets = 256,

        /// <summary>
        /// 多个目标时候启用
        /// </summary>
        EnableIfMultiTargets = 512,
        /// <summary>
        /// 多个目标的时候隐藏
        /// </summary>
        HideIfMultiTargets = 1024,

        /// <summary>
        /// 多个目标的时候显示
        /// </summary>
        ShowIfMultiTargets = 2048,


        /// <summary>
        /// 有任何目标的时候禁用
        /// </summary>
        DisableIfAnyTarget = DisableIfSingleTarget | DisableIfMultiTargets,

        /// <summary>
        /// 有任何目标的时候隐藏
        /// </summary>
        HideIfAnyTarget = HideIfSingleTarget | HideIfMultiTargets,


        /// <summary>
        /// 有任何目标的时候启用
        /// </summary>
        EnableIfAnyTarget = EnableIfSingleTarget | EnableIfMultiTargets,

        /// <summary>
        /// 有任何目标的时候显示
        /// </summary>
        ShowIfAnyTarget = ShowIfSingleTarget | ShowIfMultiTargets,

        /// <summary>
        /// 始终启用
        /// </summary>
        EnableAlwasy = EnableIfNoneTarget | EnableIfAnyTarget,

        /// <summary>
        /// 始终显示
        /// </summary>
        ShowAlwasy = ShowIfNoneTarget | ShowIfAnyTarget,
    }
}
