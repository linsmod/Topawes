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
    public partial class WhiteListForm : BaseForm
    {
        DataStorage appDataStorage;
        public WhiteListForm(string title, string fileName)
        {
            InitializeComponent();
            this.Text = title;
            appDataStorage = new AppTextDataStorage(fileName);
            appDataStorage.LoadForType(this.GetType());
            this.textBoxBlackList.Lines = WhiteList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (Owner != null) Owner.Enabled = false;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            WhiteList = string.Join(",", textBoxBlackList.Lines);
            appDataStorage.SaveForType(this.GetType());
            if (Owner != null) Owner.Enabled = true;
            this.Hide();
        }

        [DefaultValue("")]
        public static string WhiteList;
    }
}
