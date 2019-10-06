namespace ThAreaFrameConfig.WinForms
{
    partial class ThUndergroundParkingFireProofControl
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
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.comboBox_inner_frame = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBox_outer_frame = new System.Windows.Forms.ComboBox();
            this.label_merge_compartment = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.gridControl_fire_compartment = new DevExpress.XtraGrid.GridControl();
            this.gridView_fire_compartment = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn_number = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_storey = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_serial_number = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_area = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_pick = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemHyperLinkEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit();
            this.button_create_table = new System.Windows.Forms.Button();
            this.button_create_fill = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_fire_compartment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_fire_compartment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Location = new System.Drawing.Point(5, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(478, 101);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "图层指定";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button2);
            this.groupBox3.Controls.Add(this.comboBox_inner_frame);
            this.groupBox3.Location = new System.Drawing.Point(227, 24);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(238, 62);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "指定防火分区内框扣除图层";
            // 
            // button2
            // 
            this.button2.Image = global::ThAreaFrameConfig.Properties.Resources.pick_object;
            this.button2.Location = new System.Drawing.Point(177, 22);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(28, 27);
            this.button2.TabIndex = 1;
            this.button2.UseVisualStyleBackColor = true;
            // 
            // comboBox_inner_frame
            // 
            this.comboBox_inner_frame.FormattingEnabled = true;
            this.comboBox_inner_frame.Location = new System.Drawing.Point(8, 24);
            this.comboBox_inner_frame.Name = "comboBox_inner_frame";
            this.comboBox_inner_frame.Size = new System.Drawing.Size(160, 22);
            this.comboBox_inner_frame.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.comboBox_outer_frame);
            this.groupBox2.Location = new System.Drawing.Point(8, 24);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(213, 62);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "指定防火分区外框线图层";
            // 
            // button1
            // 
            this.button1.Image = global::ThAreaFrameConfig.Properties.Resources.pick_object;
            this.button1.Location = new System.Drawing.Point(177, 22);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(28, 27);
            this.button1.TabIndex = 1;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // comboBox_outer_frame
            // 
            this.comboBox_outer_frame.FormattingEnabled = true;
            this.comboBox_outer_frame.Location = new System.Drawing.Point(8, 24);
            this.comboBox_outer_frame.Name = "comboBox_outer_frame";
            this.comboBox_outer_frame.Size = new System.Drawing.Size(160, 22);
            this.comboBox_outer_frame.TabIndex = 0;
            // 
            // label_merge_compartment
            // 
            this.label_merge_compartment.AutoSize = true;
            this.label_merge_compartment.Location = new System.Drawing.Point(40, 243);
            this.label_merge_compartment.Name = "label_merge_compartment";
            this.label_merge_compartment.Size = new System.Drawing.Size(115, 14);
            this.label_merge_compartment.TabIndex = 1;
            this.label_merge_compartment.Text = "点击合并防火分区：";
            // 
            // button3
            // 
            this.button3.Image = global::ThAreaFrameConfig.Properties.Resources.pick_object;
            this.button3.Location = new System.Drawing.Point(6, 237);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(28, 27);
            this.button3.TabIndex = 0;
            this.button3.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label_merge_compartment);
            this.groupBox5.Controls.Add(this.gridControl_fire_compartment);
            this.groupBox5.Controls.Add(this.button3);
            this.groupBox5.Location = new System.Drawing.Point(5, 113);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(478, 270);
            this.groupBox5.TabIndex = 5;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "防火分区表";
            // 
            // gridControl_fire_compartment
            // 
            this.gridControl_fire_compartment.Location = new System.Drawing.Point(8, 24);
            this.gridControl_fire_compartment.MainView = this.gridView_fire_compartment;
            this.gridControl_fire_compartment.Name = "gridControl_fire_compartment";
            this.gridControl_fire_compartment.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemHyperLinkEdit1});
            this.gridControl_fire_compartment.Size = new System.Drawing.Size(464, 207);
            this.gridControl_fire_compartment.TabIndex = 5;
            this.gridControl_fire_compartment.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView_fire_compartment});
            // 
            // gridView_fire_compartment
            // 
            this.gridView_fire_compartment.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn_number,
            this.gridColumn_storey,
            this.gridColumn_serial_number,
            this.gridColumn_area,
            this.gridColumn_pick});
            this.gridView_fire_compartment.GridControl = this.gridControl_fire_compartment;
            this.gridView_fire_compartment.Name = "gridView_fire_compartment";
            this.gridView_fire_compartment.OptionsView.ShowGroupPanel = false;
            this.gridView_fire_compartment.RowClick += new DevExpress.XtraGrid.Views.Grid.RowClickEventHandler(this.gridView_fire_compartment_RowClick);
            this.gridView_fire_compartment.CustomUnboundColumnData += new DevExpress.XtraGrid.Views.Base.CustomColumnDataEventHandler(this.gridView_fire_compartment_CustomUnboundColumnData);
            this.gridView_fire_compartment.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridView_fire_compartment_CustomColumnDisplayText);
            // 
            // gridColumn_number
            // 
            this.gridColumn_number.Caption = "序号";
            this.gridColumn_number.FieldName = "Number";
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
            this.gridColumn_serial_number.VisibleIndex = 2;
            // 
            // gridColumn_area
            // 
            this.gridColumn_area.Caption = "面积";
            this.gridColumn_area.FieldName = "Area";
            this.gridColumn_area.Name = "gridColumn_area";
            this.gridColumn_area.OptionsColumn.AllowEdit = false;
            this.gridColumn_area.OptionsColumn.AllowFocus = false;
            this.gridColumn_area.Visible = true;
            this.gridColumn_area.VisibleIndex = 3;
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
            this.gridColumn_pick.VisibleIndex = 4;
            // 
            // repositoryItemHyperLinkEdit1
            // 
            this.repositoryItemHyperLinkEdit1.AutoHeight = false;
            this.repositoryItemHyperLinkEdit1.LinkColor = System.Drawing.Color.Blue;
            this.repositoryItemHyperLinkEdit1.Name = "repositoryItemHyperLinkEdit1";
            // 
            // button_create_table
            // 
            this.button_create_table.Location = new System.Drawing.Point(370, 389);
            this.button_create_table.Name = "button_create_table";
            this.button_create_table.Size = new System.Drawing.Size(113, 23);
            this.button_create_table.TabIndex = 6;
            this.button_create_table.Text = "生成防火分区表";
            this.button_create_table.UseVisualStyleBackColor = true;
            // 
            // button_create_fill
            // 
            this.button_create_fill.Location = new System.Drawing.Point(236, 389);
            this.button_create_fill.Name = "button_create_fill";
            this.button_create_fill.Size = new System.Drawing.Size(128, 23);
            this.button_create_fill.TabIndex = 7;
            this.button_create_fill.Text = "生成防火分区填充";
            this.button_create_fill.UseVisualStyleBackColor = true;
            // 
            // ThUndergroundParkingFireProofControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button_create_fill);
            this.Controls.Add(this.button_create_table);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox1);
            this.Name = "ThUndergroundParkingFireProofControl";
            this.Size = new System.Drawing.Size(500, 422);
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_fire_compartment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_fire_compartment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemHyperLinkEdit1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox comboBox_outer_frame;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox comboBox_inner_frame;
        private System.Windows.Forms.Label label_merge_compartment;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.GroupBox groupBox5;
        private DevExpress.XtraGrid.GridControl gridControl_fire_compartment;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView_fire_compartment;
        private System.Windows.Forms.Button button_create_table;
        private System.Windows.Forms.Button button_create_fill;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_number;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_storey;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_serial_number;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_area;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_pick;
        private DevExpress.XtraEditors.Repository.RepositoryItemHyperLinkEdit repositoryItemHyperLinkEdit1;
    }
}
