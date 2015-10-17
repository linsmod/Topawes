using Microsoft.Phone.Tools;
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
    public partial class LocalLoginForm : BaseForm
    {
        public event EventHandler<LocalLoginEventArgs> LocalLoginSuccess;
        public event EventHandler<LocalLoginEventArgs> LocalLoginFailed;
        public ExtendedWinFormsWebBrowser webBrowser1;
        public bool IsBusy { get; set; }
        public bool LoginEventRaised { get; private set; }
        public bool Succeeded { get; private set; }
        public List<string> Tokens { get; set; }
        public LocalLoginForm()
        {
            InitializeComponent();
            this.Load += LocalLoginForm_Load;
            this.FormClosed += LocalLoginForm_FormClosed;

        }

        private void LocalLoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!this.LoginEventRaised)
            {
                this.OnLoginFailed(new LocalLoginEventArgs { IsCancelled = true });
            }
        }

        private void OnLoginFailed(LocalLoginEventArgs args)
        {
            this.LoginEventRaised = true;
            EventHandler<LocalLoginEventArgs> signInFailed = this.LocalLoginFailed;
            if (signInFailed != null)
            {
                signInFailed(this, args);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close(false);
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        private void LocalLoginForm_Load(object sender, EventArgs e)
        {
            base.Load -= this.LocalLoginForm_Load;
            this.FormClosing += TaoLoginForm_FormClosing;
           // webBrowser1 = new Microsoft.Phone.Tools.ExtendedWinFormsWebBrowser();
            this.Text += " IE v" + webBrowser1.Version.ToString();
            webBrowser1.Dock = DockStyle.Fill;
            this.Controls.Add(webBrowser1);
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.NavigationError += WebBrowser1_NavigationError;
            //webBrowser1.ScrollBarsEnabled = false;
            //webBrowser1.IsWebBrowserContextMenuEnabled = false;
            webBrowser1.Navigated += WebBrowser1_Navigated;
            webBrowser1.DocumentCompleted += WebBrowser1_DocumentCompleted;
            webBrowser1.ProgressChanged += WebBrowser1_ProgressChanged;
            try
            {
                webBrowser1.Navigate(WinFormsClient.Server + "account/login");
            }
            catch (Exception ex)
            {
                this.OnLoginFailed(new LocalLoginEventArgs { Exception = ex });
                this.Close(false);
            }
            this.IsBusy = true;
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var email = webBrowser1.Document.GetElementById("Email");
            var password = webBrowser1.Document.GetElementById("Password");
            if (email != null && string.IsNullOrEmpty(email.GetAttribute("value")))
            {
                email.SetAttribute("value", "admin@sandsea.info");
                password.SetAttribute("value", "~Pwd123456");
                var inputs = webBrowser1.Document.GetElementsByTagName("input");
                foreach (HtmlElement input in inputs)
                {
                    if (input.GetAttribute("type") == "submit")
                    {
                        input.InvokeMember("click");
                    }
                }
            }
        }

        private void Close(bool success)
        {
            this.Succeeded = success;
            base.Close();
        }

        private void TaoLoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            webBrowser1.NavigationError -= WebBrowser1_NavigationError;
            webBrowser1.Navigated -= WebBrowser1_Navigated;
            webBrowser1.ProgressChanged -= WebBrowser1_ProgressChanged;
        }

        private void OnLoginSuccess(LocalLoginEventArgs args)
        {
            this.LoginEventRaised = true;
            EventHandler<LocalLoginEventArgs> signedIn = this.LocalLoginSuccess;
            if (signedIn != null)
            {
                signedIn(this, args);
            }
        }


        private void WebBrowser1_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            this.progressBar1.Value = (int)(e.CurrentProgress / e.MaximumProgress) * 100;
            this.progressBar1.Maximum = 100;
            this.IsBusy = (e.CurrentProgress != -1L && e.CurrentProgress < e.MaximumProgress);
        }

        private void WebBrowser1_NavigationError(object sender, EventArgs e)
        {
        }

        private void WebBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            this.Tokens = IECookieHelper.GetCookieHeaderList(webBrowser1, Tokens);
            if (e.Url.ToString().Equals(WinFormsClient.Server))
            {
                this.DialogResult = DialogResult.OK;
                this.OnLoginSuccess(new LocalLoginEventArgs
                {
                    Tokens = this.Tokens,
                });
                this.Close(true);
                return;
            }

        }
    }
    public class LocalLoginEventArgs : EventArgs
    {
        public List<string> Tokens
        {
            get;
            set;
        }
        public string ErrorCode
        {
            get;
            set;
        }
        public string ErrorDescription
        {
            get;
            set;
        }
        public bool IsCancelled
        {
            get;
            set;
        }
        public Exception Exception
        {
            get;
            set;
        }
    }
}
