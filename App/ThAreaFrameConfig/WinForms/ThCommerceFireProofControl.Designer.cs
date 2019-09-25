namespace ThAreaFrameConfig.WinForms
{
    partial class ThCommerceFireProofControl
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBox_density = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_storey = new System.Windows.Forms.TextBox();
            this.comboBox_fire_resistance = new System.Windows.Forms.ComboBox();
            this.textBox_sub_key = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.comboBox_inner_frame = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBox_outer_frame = new System.Windows.Forms.ComboBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.gridControl_fire_compartment = new DevExpress.XtraGrid.GridControl();
            this.gridView_fire_compartment = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn_number = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_storey = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_serial_number = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_area = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_self_extinguishing_system = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemImageComboBox1 = new DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox();
            this.gridColumn_pick = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemHyperLinkEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit();
            this.button3 = new System.Windows.Forms.Button();
            this.button_create_table = new System.Windows.Forms.Button();
            this.button_create_fill = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_fire_compartment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_fire_compartment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemImageComboBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBox_density);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBox_storey);
            this.groupBox1.Controls.Add(this.comboBox_fire_resistance);
            this.groupBox1.Controls.Add(this.textBox_sub_key);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(5, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(220, 147);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "建筑信息";
            // 
            // comboBox_density
            // 
            this.comboBox_density.FormattingEnabled = true;
            this.comboBox_density.Items.AddRange(new object[] {
            "下限",
            "中值",
            "上限"});
            this.comboBox_density.Location = new System.Drawing.Point(96, 113);
            this.comboBox_density.Name = "comboBox_density";
            this.comboBox_density.Size = new System.Drawing.Size(115, 22);
            this.comboBox_density.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 14);
            this.label4.TabIndex = 6;
            this.label4.Text = "人员密度：";
            // 
            // textBox_storey
            // 
            this.textBox_storey.Location = new System.Drawing.Point(95, 84);
            this.textBox_storey.Name = "textBox_storey";
            this.textBox_storey.Size = new System.Drawing.Size(116, 22);
            this.textBox_storey.TabIndex = 5;
            // 
            // comboBox_fire_resistance
            // 
            this.comboBox_fire_resistance.FormattingEnabled = true;
            this.comboBox_fire_resistance.Items.AddRange(new object[] {
            "一级",
            "二级",
            "三级",
            "四级"});
            this.comboBox_fire_resistance.Location = new System.Drawing.Point(95, 56);
            this.comboBox_fire_resistance.Name = "comboBox_fire_resistance";
            this.comboBox_fire_resistance.Size = new System.Drawing.Size(116, 22);
            this.comboBox_fire_resistance.TabIndex = 4;
            // 
            // textBox_sub_key
            // 
            this.textBox_sub_key.Location = new System.Drawing.Point(95, 28);
            this.textBox_sub_key.Name = "textBox_sub_key";
            this.textBox_sub_key.Size = new System.Drawing.Size(116, 22);
            this.textBox_sub_key.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 14);
            this.label3.TabIndex = 2;
            this.label3.Text = "地上层数：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 14);
            this.label2.TabIndex = 1;
            this.label2.Text = "耐火等级：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "子项编号：";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.groupBox4);
            this.groupBox2.Controls.Add(this.groupBox3);
            this.groupBox2.Location = new System.Drawing.Point(231, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(257, 147);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "图层指定";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.button2);
            this.groupBox4.Controls.Add(this.comboBox_inner_frame);
            this.groupBox4.Location = new System.Drawing.Point(7, 81);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(241, 59);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "指定防火分区内框扣除图层";
            // 
            // button2
            // 
            this.button2.Image = global::ThAreaFrameConfig.Properties.Resources.pick_object;
            this.button2.Location = new System.Drawing.Point(195, 24);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(28, 27);
            this.button2.TabIndex = 1;
            this.button2.UseVisualStyleBackColor = true;
            // 
            // comboBox_inner_frame
            // 
            this.comboBox_inner_frame.FormattingEnabled = true;
            this.comboBox_inner_frame.Location = new System.Drawing.Point(19, 24);
            this.comboBox_inner_frame.Name = "comboBox_inner_frame";
            this.comboBox_inner_frame.Size = new System.Drawing.Size(170, 22);
            this.comboBox_inner_frame.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.comboBox_outer_frame);
            this.groupBox3.Location = new System.Drawing.Point(7, 21);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(241, 54);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "指定防火分区外框线图层";
            // 
            // button1
            // 
            this.button1.Image = global::ThAreaFrameConfig.Properties.Resources.pick_object;
            this.button1.Location = new System.Drawing.Point(195, 21);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(28, 27);
            this.button1.TabIndex = 1;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // comboBox_outer_frame
            // 
            this.comboBox_outer_frame.FormattingEnabled = true;
            this.comboBox_outer_frame.Location = new System.Drawing.Point(19, 24);
            this.comboBox_outer_frame.Name = "comboBox_outer_frame";
            this.comboBox_outer_frame.Size = new System.Drawing.Size(170, 22);
            this.comboBox_outer_frame.TabIndex = 0;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Controls.Add(this.gridControl_fire_compartment);
            this.groupBox5.Controls.Add(this.button3);
            this.groupBox5.Location = new System.Drawing.Point(5, 158);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(483, 289);
            this.groupBox5.TabIndex = 2;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "防火分区表";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(40, 261);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(115, 14);
            this.label6.TabIndex = 1;
            this.label6.Text = "点击合并防火分区：";
            // 
            // gridControl_fire_compartment
            // 
            this.gridControl_fire_compartment.Location = new System.Drawing.Point(6, 21);
            this.gridControl_fire_compartment.MainView = this.gridView_fire_compartment;
            this.gridControl_fire_compartment.Name = "gridControl_fire_compartment";
            this.gridControl_fire_compartment.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemHyperLinkEdit1,
            this.repositoryItemImageComboBox1});
            this.gridControl_fire_compartment.Size = new System.Drawing.Size(468, 228);
            this.gridControl_fire_compartment.TabIndex = 5;
            this.gridControl_fire_compartment.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView_fire_compartment});
            // 
            // gridView_fire_compartment
            // 
            this.gridView_fire_compartment.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn_number,
            this.gridColumn_storey,
            this.gridColumn_self_extinguishing_system,
            this.gridColumn_serial_number,
            this.gridColumn_area,
            this.gridColumn_pick});
            this.gridView_fire_compartment.GridControl = this.gridControl_fire_compartment;
            this.gridView_fire_compartment.Name = "gridView_fire_compartment";
            this.gridView_fire_compartment.OptionsCustomization.AllowColumnMoving = false;
            this.gridView_fire_compartment.OptionsCustomization.AllowFilter = false;
            this.gridView_fire_compartment.OptionsCustomization.AllowGroup = false;
            this.gridView_fire_compartment.OptionsCustomization.AllowQuickHideColumns = false;
            this.gridView_fire_compartment.OptionsCustomization.AllowSort = false;
            this.gridView_fire_compartment.OptionsDetail.EnableMasterViewMode = false;
            this.gridView_fire_compartment.OptionsSelection.MultiSelect = true;
            this.gridView_fire_compartment.OptionsView.ShowGroupPanel = false;
            this.gridView_fire_compartment.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView_fire_compartment_RowClick);
            this.gridView_fire_compartment.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gridView_fire_compartment_PopupMenuShowing);
            this.gridView_fire_compartment.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gridView_fire_compartment_CellValueChanged);
            this.gridView_fire_compartment.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gridView_fire_compartment_CustomUnboundColumnData);
            this.gridView_fire_compartment.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridView_fire_compartment_CustomColumnDisplayText);
            this.gridView_fire_compartment.ValidatingEditor += new DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventHandler(this.gridView_fire_compartment_ValidatingEditor);
            // 
            // gridColumn_number
            // 
            this.gridColumn_number.Caption = "序号";
            this.gridColumn_number.FieldName = "Index";
            this.gridColumn_number.Name = "gridColumn_number";
            this.gridColumn_number.OptionsColumn.AllowEdit = false;
            this.gridColumn_number.OptionsColumn.AllowFocus = false;
            this.gridColumn_number.Visible = true;
            this.gridColumn_number.VisibleIndex = 0;
            // 
            // gridColumn_storey
            // 
            this.gridColumn_storey.Caption = "输入楼层";
            this.gridColumn_storey.FieldName = "Storey";
            this.gridColumn_storey.Name = "gridColumn_storey";
            this.gridColumn_storey.Visible = true;
            this.gridColumn_storey.VisibleIndex = 1;
            // 
            // gridColumn_serial_number
            // 
            this.gridColumn_serial_number.Caption = "防火分区编号";
            this.gridColumn_serial_number.FieldName = "SerialNumber";
            this.gridColumn_serial_number.Name = "gridColumn_serial_number";
            this.gridColumn_serial_number.OptionsColumn.AllowEdit = false;
            this.gridColumn_serial_number.OptionsColumn.AllowFocus = false;
            this.gridColumn_serial_number.Visible = true;
            this.gridColumn_serial_number.VisibleIndex = 3;
            // 
            // gridColumn_area
            // 
            this.gridColumn_area.Caption = "面积";
            this.gridColumn_area.FieldName = "Area";
            this.gridColumn_area.Name = "gridColumn_area";
            this.gridColumn_area.OptionsColumn.AllowEdit = false;
            this.gridColumn_area.OptionsColumn.AllowFocus = false;
            this.gridColumn_area.Visible = true;
            this.gridColumn_area.VisibleIndex = 4;
            // 
            // gridColumn_self_extinguishing_system
            // 
            this.gridColumn_self_extinguishing_system.Caption = "是否自动灭火系统";
            this.gridColumn_self_extinguishing_system.ColumnEdit = this.repositoryItemImageComboBox1;
            this.gridColumn_self_extinguishing_system.FieldName = "SelfExtinguishingSystem";
            this.gridColumn_self_extinguishing_system.Name = "gridColumn_self_extinguishing_system";
            this.gridColumn_self_extinguishing_system.Visible = true;
            this.gridColumn_self_extinguishing_system.VisibleIndex = 2;
            // 
            // repositoryItemImageComboBox1
            // 
            this.repositoryItemImageComboBox1.AutoHeight = false;
            this.repositoryItemImageComboBox1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemImageComboBox1.Items.AddRange(new DevExpress.XtraEditors.Controls.ImageComboBoxItem[] {
            new DevExpress.XtraEditors.Controls.ImageComboBoxItem("是", true, -1),
            new DevExpress.XtraEditors.Controls.ImageComboBoxItem("否", false, -1)});
            this.repositoryItemImageComboBox1.Name = "repositoryItemImageComboBox1";
            // 
            // gridColumn_pick
            // 
            this.gridColumn_pick.Caption = "选择框线";
            this.gridColumn_pick.ColumnEdit = this.repositoryItemHyperLinkEdit1;
            this.gridColumn_pick.FieldName = "gridColumn_pick";
            this.gridColumn_pick.Name = "gridColumn_pick";
            this.gridColumn_pick.OptionsColumn.AllowEdit = false;
            this.gridColumn_pick.OptionsColumn.AllowFocus = false;
            this.gridColumn_pick.UnboundType = DevExpress.Data.UnboundColumnType.String;
            this.gridColumn_pick.Visible = true;
            this.gridColumn_pick.VisibleIndex = 5;
            // 
            // repositoryItemHyperLinkEdit1
            // 
            this.repositoryItemHyperLinkEdit1.AutoHeight = false;
            this.repositoryItemHyperLinkEdit1.Name = "repositoryItemHyperLinkEdit1";
            // 
            // button3
            // 
            this.button3.Image = global::ThAreaFrameConfig.Properties.Resources.pick_object;
            this.button3.Location = new System.Drawing.Point(6, 255);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(28, 27);
            this.button3.TabIndex = 0;
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button_create_table
            // 
            this.button_create_table.Location = new System.Drawing.Point(375, 454);
            this.button_create_table.Name = "button_create_table";
            this.button_create_table.Size = new System.Drawing.Size(113, 23);
            this.button_create_table.TabIndex = 3;
            this.button_create_table.Text = "生成防火分区疏散表";
            this.button_create_table.UseVisualStyleBackColor = true;
            // 
            // button_create_fill
            // 
            this.button_create_fill.Location = new System.Drawing.Point(257, 454);
            this.button_create_fill.Name = "button_create_fill";
            this.button_create_fill.Size = new System.Drawing.Size(112, 23);
            this.button_create_fill.TabIndex = 4;
            this.button_create_fill.Text = "生成防火分区填充";
            this.button_create_fill.UseVisualStyleBackColor = true;
            // 
            // ThCommerceFireProofControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button_create_fill);
            this.Controls.Add(this.button_create_table);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "ThCommerceFireProofControl";
            this.Size = new System.Drawing.Size(500, 488);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_fire_compartment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_fire_compartment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemImageComboBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_sub_key;
        private System.Windows.Forms.TextBox textBox_storey;
        private System.Windows.Forms.ComboBox comboBox_fire_resistance;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBox_outer_frame;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox comboBox_inner_frame;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label6;
        private DevExpress.XtraGrid.GridControl gridControl_fire_compartment;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView_fire_compartment;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_number;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_storey;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_serial_number;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_area;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_pick;
        private DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit repositoryItemHyperLinkEdit1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_self_extinguishing_system;
        private DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox repositoryItemImageComboBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_density;
        private System.Windows.Forms.Button button_create_table;
        private System.Windows.Forms.Button button_create_fill;
    }
}
