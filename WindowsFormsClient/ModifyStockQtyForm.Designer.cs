namespace WinFormsClient
{
    partial class ModifyStockQtyForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.确定按钮 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.点卡直充库存 = new Moonlight.WindowsForms.StateControls.MoonUpDown();
            this.话费直充库存 = new Moonlight.WindowsForms.StateControls.MoonUpDown();
            this.QQ直充库存 = new Moonlight.WindowsForms.StateControls.MoonUpDown();
            this.修改QQ直充库存 = new Moonlight.WindowsForms.StateControls.MoonCheckBox();
            this.修改话费直充库存 = new Moonlight.WindowsForms.StateControls.MoonCheckBox();
            this.修改点卡直充库存 = new Moonlight.WindowsForms.StateControls.MoonCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.点卡直充库存)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.话费直充库存)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.QQ直充库存)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "QQ直充";
            // 
            // 确定按钮
            // 
            this.确定按钮.Location = new System.Drawing.Point(218, 143);
            this.确定按钮.Name = "确定按钮";
            this.确定按钮.Size = new System.Drawing.Size(75, 23);
            this.确定按钮.TabIndex = 2;
            this.确定按钮.Text = "确定";
            this.确定按钮.UseVisualStyleBackColor = true;
            this.确定按钮.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "话费直充";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "点卡直充";
            // 
            // 点卡直充库存
            // 
            this.点卡直充库存.AppUserSettingProtect = false;
            this.点卡直充库存.Location = new System.Drawing.Point(95, 102);
            this.点卡直充库存.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.点卡直充库存.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.点卡直充库存.Name = "点卡直充库存";
            this.点卡直充库存.Size = new System.Drawing.Size(120, 21);
            this.点卡直充库存.TabIndex = 3;
            this.点卡直充库存.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // 话费直充库存
            // 
            this.话费直充库存.AppUserSettingProtect = false;
            this.话费直充库存.Location = new System.Drawing.Point(95, 61);
            this.话费直充库存.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.话费直充库存.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.话费直充库存.Name = "话费直充库存";
            this.话费直充库存.Size = new System.Drawing.Size(120, 21);
            this.话费直充库存.TabIndex = 4;
            this.话费直充库存.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // QQ直充库存
            // 
            this.QQ直充库存.AppUserSettingProtect = false;
            this.QQ直充库存.Location = new System.Drawing.Point(95, 23);
            this.QQ直充库存.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.QQ直充库存.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.QQ直充库存.Name = "QQ直充库存";
            this.QQ直充库存.Size = new System.Drawing.Size(120, 21);
            this.QQ直充库存.TabIndex = 5;
            this.QQ直充库存.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // 修改QQ直充库存
            // 
            this.修改QQ直充库存.AppUserSettingProtect = false;
            this.修改QQ直充库存.AutoSize = true;
            this.修改QQ直充库存.Checked = true;
            this.修改QQ直充库存.CheckState = System.Windows.Forms.CheckState.Checked;
            this.修改QQ直充库存.Location = new System.Drawing.Point(221, 25);
            this.修改QQ直充库存.Name = "修改QQ直充库存";
            this.修改QQ直充库存.Size = new System.Drawing.Size(72, 16);
            this.修改QQ直充库存.TabIndex = 6;
            this.修改QQ直充库存.Text = "本次处理";
            this.修改QQ直充库存.UseVisualStyleBackColor = true;
            // 
            // 修改话费直充库存
            // 
            this.修改话费直充库存.AppUserSettingProtect = false;
            this.修改话费直充库存.AutoSize = true;
            this.修改话费直充库存.Checked = true;
            this.修改话费直充库存.CheckState = System.Windows.Forms.CheckState.Checked;
            this.修改话费直充库存.Location = new System.Drawing.Point(221, 63);
            this.修改话费直充库存.Name = "修改话费直充库存";
            this.修改话费直充库存.Size = new System.Drawing.Size(72, 16);
            this.修改话费直充库存.TabIndex = 6;
            this.修改话费直充库存.Text = "本次处理";
            this.修改话费直充库存.UseVisualStyleBackColor = true;
            // 
            // 修改点卡直充库存
            // 
            this.修改点卡直充库存.AppUserSettingProtect = false;
            this.修改点卡直充库存.AutoSize = true;
            this.修改点卡直充库存.Checked = true;
            this.修改点卡直充库存.CheckState = System.Windows.Forms.CheckState.Checked;
            this.修改点卡直充库存.Location = new System.Drawing.Point(221, 104);
            this.修改点卡直充库存.Name = "修改点卡直充库存";
            this.修改点卡直充库存.Size = new System.Drawing.Size(72, 16);
            this.修改点卡直充库存.TabIndex = 6;
            this.修改点卡直充库存.Text = "本次处理";
            this.修改点卡直充库存.UseVisualStyleBackColor = true;
            // 
            // ModifyStockQtyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 187);
            this.Controls.Add(this.修改点卡直充库存);
            this.Controls.Add(this.修改话费直充库存);
            this.Controls.Add(this.修改QQ直充库存);
            this.Controls.Add(this.QQ直充库存);
            this.Controls.Add(this.话费直充库存);
            this.Controls.Add(this.点卡直充库存);
            this.Controls.Add(this.确定按钮);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ModifyStockQtyForm";
            this.Text = "ModifyStockQtyForm";
            ((System.ComponentModel.ISupportInitialize)(this.点卡直充库存)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.话费直充库存)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.QQ直充库存)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button 确定按钮;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private Moonlight.WindowsForms.StateControls.MoonUpDown 点卡直充库存;
        private Moonlight.WindowsForms.StateControls.MoonUpDown 话费直充库存;
        private Moonlight.WindowsForms.StateControls.MoonUpDown QQ直充库存;
        private Moonlight.WindowsForms.StateControls.MoonCheckBox 修改QQ直充库存;
        private Moonlight.WindowsForms.StateControls.MoonCheckBox 修改话费直充库存;
        private Moonlight.WindowsForms.StateControls.MoonCheckBox 修改点卡直充库存;
    }
}