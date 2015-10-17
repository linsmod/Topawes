using Microsoft.Phone.Tools;
using mshtml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsClient.Extensions;
using WinFormsClient.WBMode;

namespace WinFormsClient
{
    public partial class TaoLoginForm : BaseForm
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        /// <summary>
        /// 提醒选择已登录账号
        /// </summary>
        public event Action AskSelectAccount;
        /// <summary>
        /// 用户没有选择已登录账号
        /// </summary>
        public event Action UserCancelSelectAccount;
        public WBTaoLoginMode wbMode;
        public TaoLoginForm(WBTaoLoginMode wbMode)
        {
            this.wbMode = wbMode;
            InitializeComponent();
            this.Load += (s, e) => { wbMode.Navigate("https://login.taobao.com/member/login.jhtml?sign=rm402NWkiXFspfo5MW2bPQ%3D%3D&timestamp=2015-09-20+15%3A25%3A51&sub=true&style=mini_top&need_sign=top&full_redirect=true&from=mini_top&from_encoding=utf-8&TPL_redirect_url=http%3A%2F%2Fcontainer.api.taobao.com%2Fcontainer%3Fappkey%3D23140690"); };
            wbMode.AskHideUI += () => this.Hide();
            wbMode.WB.ProgressChanged2 += WbMode_ProgressChanged;
            this.VisibleChanged += TaoLoginForm_VisibleChanged;
            this.Controls.Add(wbMode.WB);

            this.Text += " [IE v" + wbMode.WB.Version.ToString() + "]";
            this.HelpButtonClicked += TaoLoginForm_HelpButtonClicked;
        }

        private void TaoLoginForm_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            MessageBox.Show("自动登陆卡顿超过5秒请按F5刷新。");
        }

        private void TaoLoginForm_VisibleChanged(object sender, EventArgs e)
        {
            this.Owner.Enabled = !this.Visible;
        }

        private void WbMode_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
            this.progressBar1.Maximum = 100;
        }
        [DebuggerStepThrough]
        [DebuggerHidden]
        protected override void WndProc(ref Message msg)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;
            const int SC_MINIMIZE = 61472;
            if (msg.Msg == WM_SYSCOMMAND && ((int)msg.WParam == SC_CLOSE))
            {
                // 点击winform右上关闭按钮 
                // 加入想要的逻辑处理
                this.wbMode.OnUserCancelLogin();
                this.DialogResult = DialogResult.Cancel;
            }
            base.WndProc(ref msg);
        }


        private void WB_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.ToString() == "http://container.api.taobao.com/container?appkey=23140690")
            {
                if (wbMode.WB.DocumentText.IndexOf("auther('true')") != -1)
                {
                    wbMode.WB.Document.InvokeScript("auther", new object[] { "true" });
                    return;
                }
            }
            if (e.Url.ToString().IndexOf("https://login.taobao.com/member/login.jhtml") != 1)
            {
                var userNameEl = wbMode.WB.Document.GetElementById("TPL_username_1");

                if (userNameEl != null)
                {
                    var passwordEl = wbMode.WB.Document.GetElementById("TPL_password_1");
                    userNameEl.SetAttribute("value", this.UserName);
                    passwordEl.SetAttribute("value", this.Password);

                    //如果验证码窗口没有显示就点击提交按钮
                    if (wbMode.WB.Document.Body.JQuerySelect(".field-checkcode.hidden").Any())
                    {
                        wbMode.WB.Document.All["J_SubmitStatic"].InvokeMember("click");
                    }
                    return;
                }
            }
            var html = HtmlHelper.GetDocumentText(wbMode.WB);
            if (html.IndexOf("选择其中一个已登录的账户") != -1)
            {
                this.OnAskSelectAccount();
                //J_SubmitQuick
            }
        }

        private void OnAskSelectAccount()
        {
            var h = this.AskSelectAccount;
            if (h != null)
            {
                h();
            }
        }
    }
}
