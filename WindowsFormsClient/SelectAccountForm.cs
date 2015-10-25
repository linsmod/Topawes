using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsClient
{
    public partial class SelectAccountForm : BaseForm
    {
        public SelectAccountForm()
        {
            InitializeComponent();
            this.Text = "选择账号";
            this.Shown += SelectAccountForm_Shown;
        }

        private void SelectAccountForm_Shown(object sender, EventArgs e)
        {
            foreach (var item in AccountButtons)
            {
                item.Height = 50;
                item.Width = this.ClientRectangle.Width - 7;
                this.flowLayoutPanel1.Controls.Add(item);
            }
        }

        public List<Button> AccountButtons = new List<Button>();
    }
}
