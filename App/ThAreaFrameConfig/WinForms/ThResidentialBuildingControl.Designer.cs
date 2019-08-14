namespace ThAreaFrameConfig.WinForms
{
    partial class ThResidentialBuildingControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.labelControl_number = new DevExpress.XtraEditors.LabelControl();
            this.textEdit_number = new DevExpress.XtraEditors.TextEdit();
            this.labelControl_name = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.textEdit_name = new DevExpress.XtraEditors.TextEdit();
            this.textEdit_above_ground_storeys = new DevExpress.XtraEditors.TextEdit();
            this.textEdit_under_ground_storeys = new DevExpress.XtraEditors.TextEdit();
            this.comboBoxEdit_category = new DevExpress.XtraEditors.ComboBoxEdit();
            this.simpleButton_OK = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButton_modify = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_number.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_name.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_above_ground_storeys.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_under_ground_storeys.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit_category.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // labelControl_number
            // 
            this.labelControl_number.Location = new System.Drawing.Point(39, 30);
            this.labelControl_number.Name = "labelControl_number";
            this.labelControl_number.Size = new System.Drawing.Size(60, 14);
            this.labelControl_number.TabIndex = 0;
            this.labelControl_number.Text = "建筑编号：";
            // 
            // textEdit_number
            // 
            this.textEdit_number.Location = new System.Drawing.Point(106, 30);
            this.textEdit_number.Name = "textEdit_number";
            this.textEdit_number.Size = new System.Drawing.Size(130, 20);
            this.textEdit_number.TabIndex = 1;
            // 
            // labelControl_name
            // 
            this.labelControl_name.Location = new System.Drawing.Point(39, 65);
            this.labelControl_name.Name = "labelControl_name";
            this.labelControl_name.Size = new System.Drawing.Size(60, 14);
            this.labelControl_name.TabIndex = 2;
            this.labelControl_name.Text = "建筑名称：";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(39, 99);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(60, 14);
            this.labelControl2.TabIndex = 3;
            this.labelControl2.Text = "单体性质：";
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(39, 132);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(60, 14);
            this.labelControl3.TabIndex = 4;
            this.labelControl3.Text = "地上层数：";
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(39, 166);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(60, 14);
            this.labelControl4.TabIndex = 5;
            this.labelControl4.Text = "地下层数：";
            // 
            // textEdit_name
            // 
            this.textEdit_name.Location = new System.Drawing.Point(106, 62);
            this.textEdit_name.Name = "textEdit_name";
            this.textEdit_name.Size = new System.Drawing.Size(130, 20);
            this.textEdit_name.TabIndex = 6;
            // 
            // textEdit_above_ground_storeys
            // 
            this.textEdit_above_ground_storeys.Location = new System.Drawing.Point(106, 129);
            this.textEdit_above_ground_storeys.Name = "textEdit_above_ground_storeys";
            this.textEdit_above_ground_storeys.Size = new System.Drawing.Size(130, 20);
            this.textEdit_above_ground_storeys.TabIndex = 8;
            // 
            // textEdit_under_ground_storeys
            // 
            this.textEdit_under_ground_storeys.Location = new System.Drawing.Point(106, 166);
            this.textEdit_under_ground_storeys.Name = "textEdit_under_ground_storeys";
            this.textEdit_under_ground_storeys.Size = new System.Drawing.Size(130, 20);
            this.textEdit_under_ground_storeys.TabIndex = 9;
            // 
            // comboBoxEdit_category
            // 
            this.comboBoxEdit_category.Location = new System.Drawing.Point(106, 99);
            this.comboBoxEdit_category.Name = "comboBoxEdit_category";
            this.comboBoxEdit_category.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.comboBoxEdit_category.Properties.Items.AddRange(new object[] {
            "住宅",
            "公建",
            "混合"});
            this.comboBoxEdit_category.Size = new System.Drawing.Size(130, 20);
            this.comboBoxEdit_category.TabIndex = 10;
            // 
            // simpleButton_OK
            // 
            this.simpleButton_OK.Location = new System.Drawing.Point(39, 203);
            this.simpleButton_OK.Name = "simpleButton_OK";
            this.simpleButton_OK.Size = new System.Drawing.Size(75, 23);
            this.simpleButton_OK.TabIndex = 11;
            this.simpleButton_OK.Text = "定义";
            this.simpleButton_OK.Click += new System.EventHandler(this.simpleButton_OK_Click);
            // 
            // simpleButton_modify
            // 
            this.simpleButton_modify.Location = new System.Drawing.Point(161, 203);
            this.simpleButton_modify.Name = "simpleButton_modify";
            this.simpleButton_modify.Size = new System.Drawing.Size(75, 23);
            this.simpleButton_modify.TabIndex = 12;
            this.simpleButton_modify.Text = "修改";
            this.simpleButton_modify.Click += new System.EventHandler(this.simpleButton_modify_Click);
            // 
            // ThResidentialBuildingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.simpleButton_modify);
            this.Controls.Add(this.simpleButton_OK);
            this.Controls.Add(this.comboBoxEdit_category);
            this.Controls.Add(this.textEdit_under_ground_storeys);
            this.Controls.Add(this.textEdit_above_ground_storeys);
            this.Controls.Add(this.textEdit_name);
            this.Controls.Add(this.labelControl4);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.labelControl_name);
            this.Controls.Add(this.textEdit_number);
            this.Controls.Add(this.labelControl_number);
            this.Name = "ThResidentialBuildingControl";
            this.Size = new System.Drawing.Size(270, 243);
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_number.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_name.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_above_ground_storeys.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit_under_ground_storeys.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit_category.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl labelControl_number;
        private DevExpress.XtraEditors.TextEdit textEdit_number;
        private DevExpress.XtraEditors.LabelControl labelControl_name;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.TextEdit textEdit_name;
        private DevExpress.XtraEditors.TextEdit textEdit_above_ground_storeys;
        private DevExpress.XtraEditors.TextEdit textEdit_under_ground_storeys;
        private DevExpress.XtraEditors.ComboBoxEdit comboBoxEdit_category;
        private DevExpress.XtraEditors.SimpleButton simpleButton_OK;
        private DevExpress.XtraEditors.SimpleButton simpleButton_modify;
    }
}
