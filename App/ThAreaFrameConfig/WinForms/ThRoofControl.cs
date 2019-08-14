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
    public partial class ThRoofControl : DevExpress.XtraEditors.XtraUserControl, IRoofView
    {
        private ThRoofPresenter Presenter;
        private ThRoofDbRepository DbRepository;


        public ThRoofControl()
        {
            InitializeComponent();
            InitializeGridControl();
        }

        public List<ThRoof> Roofs
        {
            get
            {
                return (List<ThRoof>)gridControl_roof.DataSource;
            }

            set
            {
                gridControl_roof.DataSource = value;
            }
        }

        public void Attach(IRoofPresenterCallback presenter)
        {
            //
        }

        public void Reload()
        {
            //
        }

        public void InitializeGridControl()
        {
            Presenter = new ThRoofPresenter(this);
            DbRepository = new ThRoofDbRepository();
            if (DbRepository.Roofs.Count == 0)
            {
                DbRepository.AppendRoof();
            }
            gridControl_roof.DataSource = DbRepository.Roofs;
            gridControl_roof.RefreshDataSource();
        }
    }
}
