namespace ThAreaFrameConfig.WinForms
{
    partial class ThOutdoorParkingSpace
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
            this.gridControl_outdoor_parking_space = new DevExpress.XtraGrid.GridControl();
            this.gridView_outdoor_parking_space = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_outdoor_parking_space)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_outdoor_parking_space)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl_outdoor_parking_space
            // 
            this.gridControl_outdoor_parking_space.Location = new System.Drawing.Point(4, 4);
            this.gridControl_outdoor_parking_space.MainView = this.gridView_outdoor_parking_space;
            this.gridControl_outdoor_parking_space.Name = "gridControl_outdoor_parking_space";
            this.gridControl_outdoor_parking_space.Size = new System.Drawing.Size(545, 339);
            this.gridControl_outdoor_parking_space.TabIndex = 0;
            this.gridControl_outdoor_parking_space.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView_outdoor_parking_space});
            // 
            // gridView_outdoor_parking_space
            // 
            this.gridView_outdoor_parking_space.GridControl = this.gridControl_outdoor_parking_space;
            this.gridView_outdoor_parking_space.Name = "gridView_outdoor_parking_space";
            this.gridView_outdoor_parking_space.OptionsView.ShowGroupPanel = false;
            // 
            // ThOutdoorParkingSpace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl_outdoor_parking_space);
            this.Name = "ThOutdoorParkingSpace";
            this.Size = new System.Drawing.Size(552, 346);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_outdoor_parking_space)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_outdoor_parking_space)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl_outdoor_parking_space;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView_outdoor_parking_space;
    }
}
