using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsClient
{
    public class DataGridHelper
    {
        public static DataGridViewCheckBoxColumn NewDataGridViewCheckBoxColumn(string name, string text, int Width, bool isreadonly)
        {
            DataGridViewCheckBoxColumn dgvcbx = new DataGridViewCheckBoxColumn();
            dgvcbx.DataPropertyName = name;
            dgvcbx.HeaderText = text;
            dgvcbx.Width = Width;
            dgvcbx.ReadOnly = isreadonly;
            return dgvcbx;
        }

        public static DataGridViewTextBoxColumn NewDataGridViewTextBoxColumn(string name, string text, int Width, bool isreadonly)
        {
            DataGridViewTextBoxColumn dgvtbx = new DataGridViewTextBoxColumn();
            dgvtbx.DataPropertyName = name;
            dgvtbx.HeaderText = text;
            dgvtbx.Width = Width;
            dgvtbx.ReadOnly = isreadonly;
            return dgvtbx;
        }

        public static DataGridViewComboBoxColumn NewDataGridViewComboBoxColumn(string name, string text, int Width, bool isreadonly)
        {
            DataGridViewComboBoxColumn dgvcbx = new DataGridViewComboBoxColumn();
            dgvcbx.DataPropertyName = name;
            dgvcbx.HeaderText = text;
            dgvcbx.Width = Width;
            dgvcbx.ReadOnly = isreadonly;
            dgvcbx.Items.Add("1分钱递增");
            dgvcbx.Items.Add("利润不变");
            return dgvcbx;
        }

        public static void DataGridColumnsAdd(DataGridView dgv, params DataGridViewColumn[] dgvColumns)
        {
            dgv.Columns.AddRange(dgvColumns);
        }
    }
}
