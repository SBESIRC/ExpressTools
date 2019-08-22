using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Presenter;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThPlotAreaControl : DevExpress.XtraEditors.XtraUserControl, IPlotSpaceView
    {
        private ThPlotSpacePresenter Presenter;
        private ThPlotSpaceNullRepository DbRepository;


        public ThPlotAreaControl()
        {
            InitializeComponent();
            InitializeGridControl();
        }

        public List<ThPlotSpace> Spaces {
            get
            {
                return (List<ThPlotSpace>)gridControl_plot_area.DataSource;
            }

            set
            {
                gridControl_plot_area.DataSource = value;
            }
        }

        public void Attach(IPlotSpaceCallback presenter)
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
            DbRepository = new ThPlotSpaceNullRepository();
            DbRepository.AppendDefaultPlotSpace();
            gridControl_plot_area.DataSource = DbRepository.Spaces;
            gridControl_plot_area.RefreshDataSource();
        }

        public void InitializeGridControl()
        {
            Presenter = new ThPlotSpacePresenter(this);
            DbRepository = new ThPlotSpaceNullRepository();
            DbRepository.AppendDefaultPlotSpace();
            gridControl_plot_area.DataSource = DbRepository.Spaces;
            gridControl_plot_area.RefreshDataSource();
        }
    }
}
