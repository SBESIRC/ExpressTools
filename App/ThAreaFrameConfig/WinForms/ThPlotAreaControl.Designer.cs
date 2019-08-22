namespace ThAreaFrameConfig.WinForms
{
    partial class ThPlotAreaControl
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
            this.gridView_plot_area = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridControl_plot_area = new DevExpress.XtraGrid.GridControl();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_plot_area)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_plot_area)).BeginInit();
            this.SuspendLayout();
            // 
            // gridView_plot_area
            // 
            this.gridView_plot_area.GridControl = this.gridControl_plot_area;
            this.gridView_plot_area.Name = "gridView_plot_area";
            this.gridView_plot_area.OptionsView.ShowGroupPanel = false;
            // 
            // gridControl_plot_area
            // 
            this.gridControl_plot_area.Location = new System.Drawing.Point(4, 4);
            this.gridControl_plot_area.MainView = this.gridView_plot_area;
            this.gridControl_plot_area.Name = "gridControl_plot_area";
            this.gridControl_plot_area.Size = new System.Drawing.Size(542, 335);
            this.gridControl_plot_area.TabIndex = 0;
            this.gridControl_plot_area.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView_plot_area});
            // 
            // ThPlotAreaControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl_plot_area);
            this.Name = "ThPlotAreaControl";
            this.Size = new System.Drawing.Size(549, 342);
            ((System.ComponentModel.ISupportInitialize)(this.gridView_plot_area)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_plot_area)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.Views.Grid.GridView gridView_plot_area;
        private DevExpress.XtraGrid.GridControl gridControl_plot_area;
    }
}
