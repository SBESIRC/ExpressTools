namespace ThAreaFrameConfig.WinForms
{
    partial class ThPublicGreenSpace
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
            this.gridControl_public_green_space = new DevExpress.XtraGrid.GridControl();
            this.gridView_public_green_space = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_public_green_space)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_public_green_space)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl_public_green_space
            // 
            this.gridControl_public_green_space.Location = new System.Drawing.Point(4, 4);
            this.gridControl_public_green_space.MainView = this.gridView_public_green_space;
            this.gridControl_public_green_space.Name = "gridControl_public_green_space";
            this.gridControl_public_green_space.Size = new System.Drawing.Size(522, 322);
            this.gridControl_public_green_space.TabIndex = 0;
            this.gridControl_public_green_space.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView_public_green_space});
            // 
            // gridView_public_green_space
            // 
            this.gridView_public_green_space.GridControl = this.gridControl_public_green_space;
            this.gridView_public_green_space.Name = "gridView_public_green_space";
            this.gridView_public_green_space.OptionsView.ShowGroupPanel = false;
            // 
            // ThPublicGreenSpace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl_public_green_space);
            this.Name = "ThPublicGreenSpace";
            this.Size = new System.Drawing.Size(529, 329);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_public_green_space)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_public_green_space)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl_public_green_space;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView_public_green_space;
    }
}
