namespace ThAreaFrameConfig.WinForms
{
    partial class ThSitePlanDialog
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
            this.tabPane1 = new DevExpress.XtraBars.Navigation.TabPane();
            this.tabNavigationPage_plot_area = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            this.tabNavigationPage_public_green_space = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            this.tabNavigationPage_outdoor_parking_space = new DevExpress.XtraBars.Navigation.TabNavigationPage();
            this.xtraUserControl_plot_area = new ThPlotAreaControl();
            this.xtraUserControl_public_green_space = new ThPublicGreenSpace();
            this.xtraUserControl_outdoor_parking_space = new ThOutdoorParkingSpace();
            ((System.ComponentModel.ISupportInitialize)(this.tabPane1)).BeginInit();
            this.tabPane1.SuspendLayout();
            this.tabNavigationPage_plot_area.SuspendLayout();
            this.tabNavigationPage_public_green_space.SuspendLayout();
            this.tabNavigationPage_outdoor_parking_space.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPane1
            // 
            this.tabPane1.Controls.Add(this.tabNavigationPage_plot_area);
            this.tabPane1.Controls.Add(this.tabNavigationPage_public_green_space);
            this.tabPane1.Controls.Add(this.tabNavigationPage_outdoor_parking_space);
            this.tabPane1.Location = new System.Drawing.Point(12, 12);
            this.tabPane1.Name = "tabPane1";
            this.tabPane1.Pages.AddRange(new DevExpress.XtraBars.Navigation.NavigationPageBase[] {
            this.tabNavigationPage_plot_area,
            this.tabNavigationPage_public_green_space,
            this.tabNavigationPage_outdoor_parking_space});
            this.tabPane1.RegularSize = new System.Drawing.Size(460, 237);
            this.tabPane1.SelectedPage = this.tabNavigationPage_plot_area;
            this.tabPane1.Size = new System.Drawing.Size(460, 237);
            this.tabPane1.TabIndex = 0;
            this.tabPane1.Text = "tabPane1";
            // 
            // tabNavigationPage_plot_area
            // 
            this.tabNavigationPage_plot_area.Caption = "规划净用地";
            this.tabNavigationPage_plot_area.Controls.Add(this.xtraUserControl_plot_area);
            this.tabNavigationPage_plot_area.Name = "tabNavigationPage_plot_area";
            this.tabNavigationPage_plot_area.Size = new System.Drawing.Size(442, 191);
            // 
            // tabNavigationPage_public_green_space
            // 
            this.tabNavigationPage_public_green_space.Caption = "公共绿地";
            this.tabNavigationPage_public_green_space.Controls.Add(this.xtraUserControl_public_green_space);
            this.tabNavigationPage_public_green_space.Name = "tabNavigationPage_public_green_space";
            this.tabNavigationPage_public_green_space.Size = new System.Drawing.Size(442, 191);
            // 
            // tabNavigationPage_outdoor_parking_space
            // 
            this.tabNavigationPage_outdoor_parking_space.Caption = "室外车位";
            this.tabNavigationPage_outdoor_parking_space.Controls.Add(this.xtraUserControl_outdoor_parking_space);
            this.tabNavigationPage_outdoor_parking_space.Name = "tabNavigationPage_outdoor_parking_space";
            this.tabNavigationPage_outdoor_parking_space.Size = new System.Drawing.Size(442, 191);
            // 
            // xtraUserControl_plot_area
            // 
            this.xtraUserControl_plot_area.Location = new System.Drawing.Point(4, 4);
            this.xtraUserControl_plot_area.Name = "xtraUserControl_plot_area";
            this.xtraUserControl_plot_area.Size = new System.Drawing.Size(435, 184);
            this.xtraUserControl_plot_area.TabIndex = 0;
            // 
            // xtraUserControl_public_green_space
            // 
            this.xtraUserControl_public_green_space.Location = new System.Drawing.Point(4, 4);
            this.xtraUserControl_public_green_space.Name = "xtraUserControl_public_green_space";
            this.xtraUserControl_public_green_space.Size = new System.Drawing.Size(435, 184);
            this.xtraUserControl_public_green_space.TabIndex = 0;
            // 
            // xtraUserControl_outdoor_parking_space
            // 
            this.xtraUserControl_outdoor_parking_space.Location = new System.Drawing.Point(4, 4);
            this.xtraUserControl_outdoor_parking_space.Name = "xtraUserControl_outdoor_parking_space";
            this.xtraUserControl_outdoor_parking_space.Size = new System.Drawing.Size(435, 184);
            this.xtraUserControl_outdoor_parking_space.TabIndex = 0;
            // 
            // ThSitePlanDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 261);
            this.Controls.Add(this.tabPane1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ThSitePlanDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "总平规整";
            ((System.ComponentModel.ISupportInitialize)(this.tabPane1)).EndInit();
            this.tabPane1.ResumeLayout(false);
            this.tabNavigationPage_plot_area.ResumeLayout(false);
            this.tabNavigationPage_public_green_space.ResumeLayout(false);
            this.tabNavigationPage_outdoor_parking_space.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraBars.Navigation.TabPane tabPane1;
        private DevExpress.XtraBars.Navigation.TabNavigationPage tabNavigationPage_plot_area;
        private DevExpress.XtraBars.Navigation.TabNavigationPage tabNavigationPage_public_green_space;
        private DevExpress.XtraBars.Navigation.TabNavigationPage tabNavigationPage_outdoor_parking_space;
        private ThPlotAreaControl xtraUserControl_plot_area;
        private ThPublicGreenSpace xtraUserControl_public_green_space;
        private ThOutdoorParkingSpace xtraUserControl_outdoor_parking_space;
    }
}