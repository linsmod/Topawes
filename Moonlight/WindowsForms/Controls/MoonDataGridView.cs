using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using Moonlight.WindowsForms.StateControls;

namespace Moonlight.WindowsForms.Controls
{
    public partial class MoonDataGridView : DataGridView
    {
        public bool IsMouseOnCell { get; set; }
        public MoonDataGridView()
        {
            InitializeComponent();
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.DefaultCellStyle.SelectionBackColor = Color.SteelBlue;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            BackgroundColor = System.Drawing.Color.WhiteSmoke;
            ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.CellMouseEnter += MoonDataGridView_CellMouseEnter;
            this.CellMouseLeave += MoonDataGridView_CellMouseLeave;
            this.Click += MoonDataGridView_Click;
            this.RowPostPaint += dataGridView1_RowPostPaint;
            this.ContextMenuStripChanged += MoonDataGridView_ContextMenuStripChanged;
            this.RowContextMenuStripNeeded += MoonDataGridView_RowContextMenuStripNeeded;
            this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
        }

        private void MoonDataGridView_RowContextMenuStripNeeded(object sender, DataGridViewRowContextMenuStripNeededEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (e.RowIndex != -1)
            {
                if (!dgv.Rows[e.RowIndex].Selected)
                {
                    dgv.ClearSelection();
                    dgv.Rows[e.RowIndex].Selected = true;
                }
                e.ContextMenuStrip = ContextMenuStrip;
            }
        }

        private void MoonDataGridView_Click(object sender, EventArgs e)
        {
            if (!IsMouseOnCell)
                this.ClearSlectionIfControlKeyNotPressDown();
        }

        private void MoonDataGridView_ContextMenuStripChanged(object sender, EventArgs e)
        {
            if (this.ContextMenuStrip != null)
            {
                this.ContextMenuStrip.Opening -= ContextMenuStrip_Opening;
                this.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
                this.ContextMenuStrip.Closing -= ContextMenuStrip_Closing;
                this.ContextMenuStrip.Closing += ContextMenuStrip_Closing;
            }
        }

        private void ContextMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            foreach (var item in ContextMenuStrip.Items)
            {
                var moonItem = item as Moonlight.WindowsForms.StateControls.MoonToolStripMenuItem;
                if (moonItem != null)
                {
                    moonItem.OnContextMenuStripClosing();
                }
            }
        }

        private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (!IsMouseOnCell)
                this.ClearSlectionIfControlKeyNotPressDown();
            foreach (var item in ContextMenuStrip.Items)
            {
                var moonItem = item as Moonlight.WindowsForms.StateControls.MoonToolStripMenuItem;
                if (moonItem != null)
                {
                    moonItem.OnContextMenuStripOpening(this.SelectedRows);
                }
            }
        }

        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (e.RowIndex % 10 == 0)
            {
                Rectangle rectangle = new Rectangle(e.RowBounds.Location.X, e.RowBounds.Location.Y, RowHeadersWidth - 4, e.RowBounds.Height);
                TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                    RowHeadersDefaultCellStyle.Font,
                    rectangle,
                    RowHeadersDefaultCellStyle.ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
            }
        }

        /// <summary>
        /// 反选
        /// </summary>
        public void InvertSelection()
        {
            var rows = Rows.AsList<DataGridViewRow>();
            foreach (var item in rows)
            {
                item.Selected = !item.Selected;
            }
        }

        private void MoonDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;
            IsMouseOnCell = false;
            if (e.RowIndex > -1)
            {
                if (!dgv.Rows[e.RowIndex].Selected)
                {
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = SystemColors.Window;
                }
            }
        }

        private void MoonDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;
            IsMouseOnCell = true;
            if (e.RowIndex > -1)
            {
                if (!dgv.Rows[e.RowIndex].Selected)
                {
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = SystemColors.Control;
                }
            }
        }

        /// <summary>
        /// 如果没有按下Control键就执行ClearSelection,一般用于增选行数据
        /// </summary>
        public void ClearSlectionIfControlKeyNotPressDown()
        {
            if (IsControlKeyPressedDown)
            {
                return;
            }
            this.ClearSelection();
        }

        /// <summary>
        /// 判断CTRL键是否按下
        /// </summary>
        public bool IsControlKeyPressedDown
        {
            get
            {
                // CTRL is pressed
                return (Control.ModifierKeys & Keys.Control) == Keys.Control;
            }
        }
    }
}
