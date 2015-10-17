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
    public partial class BlackListForm : BaseForm
    {
        DataStorage appDataStorage;
        public BlackListForm(string title, string fileName)
        {
            InitializeComponent();
            this.Text = title;
            appDataStorage = new AppTextDataStorage(fileName);
            appDataStorage.LoadForType(this.GetType());
            this.textBoxBlackList.Lines = BlackList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (Owner != null) Owner.Enabled = false;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            BlackList = string.Join(",", textBoxBlackList.Lines);
            appDataStorage.SaveForType(this.GetType());
            if (Owner != null) Owner.Enabled = true;
            this.Hide();
        }

        [DefaultValue("")]
        public static string BlackList;
    }
}
