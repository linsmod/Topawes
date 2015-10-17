using Moonlight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsClient
{
    public partial class EditTextLinesForm : Form
    {
        public Action SavedCallback;
        public string StorageKey { get; set; }
        public EditTextLinesForm(string title, string storageKey, string[] lines, Action savedCallback)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            if (Owner != null) Owner.Enabled = false;
            this.StorageKey = storageKey;
            this.VisibleChanged += (s, e) => { };
            Text = title;
            this.textBoxBlackList.Lines = AppSetting.UserSetting.Get<string[]>(storageKey, lines);
            this.SavedCallback = savedCallback;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            AppSetting.UserSetting.Set<string[]>(StorageKey, this.textBoxBlackList.Lines);
            if (SavedCallback != null)
            {
                SavedCallback();
                var h = SavedCallback;
            }
            if (Owner != null) Owner.Enabled = true;
            this.Close();
        }
    }
}
