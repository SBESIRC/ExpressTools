﻿namespace ThAreaFrameConfig.WinForms
{
    partial class ThRoofGreenSpaceControl
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
            this.gridControl_space = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn_number = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_coefficient = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn_area = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_space)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl_space
            // 
            this.gridControl_space.Location = new System.Drawing.Point(4, 4);
            this.gridControl_space.MainView = this.gridView1;
            this.gridControl_space.Name = "gridControl_space";
            this.gridControl_space.Size = new System.Drawing.Size(682, 502);
            this.gridControl_space.TabIndex = 0;
            this.gridControl_space.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn_number,
            this.gridColumn_coefficient,
            this.gridColumn_area});
            this.gridView1.GridControl = this.gridControl_space;
            this.gridView1.Name = "gridView1";
            // 
            // gridColumn_number
            // 
            this.gridColumn_number.Caption = "编号";
            this.gridColumn_number.FieldName = "Number";
            this.gridColumn_number.Name = "gridColumn_number";
            this.gridColumn_number.Visible = true;
            this.gridColumn_number.VisibleIndex = 0;
            // 
            // gridColumn_coefficient
            // 
            this.gridColumn_coefficient.Caption = "折算系数";
            this.gridColumn_coefficient.FieldName = "Coefficient";
            this.gridColumn_coefficient.Name = "gridColumn_coefficient";
            this.gridColumn_coefficient.Visible = true;
            this.gridColumn_coefficient.VisibleIndex = 1;
            // 
            // gridColumn_area
            // 
            this.gridColumn_area.Caption = "面积";
            this.gridColumn_area.FieldName = "Area";
            this.gridColumn_area.Name = "gridColumn_area";
            this.gridColumn_area.Visible = true;
            this.gridColumn_area.VisibleIndex = 2;
            // 
            // ThRoofGreenSpaceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl_space);
            this.Name = "ThRoofGreenSpaceControl";
            this.Size = new System.Drawing.Size(689, 509);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_space)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl_space;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_number;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_coefficient;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn_area;
    }
}
