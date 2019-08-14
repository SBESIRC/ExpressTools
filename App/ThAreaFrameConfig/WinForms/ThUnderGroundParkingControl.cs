using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Presenter;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThUnderGroundParkingControl : DevExpress.XtraEditors.XtraUserControl, IUnderGroundParkingView
    {
        private ThUnderGroundParkingPresenter Presenter;
        private ThUnderGroundParkingDbRepository DbRepository;

        public ThUnderGroundParkingControl()
        {
            InitializeComponent();
            InitializeGridControl();
        }

        public void Attach(IUnderGroundParkingPresenterCallback presenter)
        {
            //
        }

        public List<ThUnderGroundParking> Parkings
        {
            get
            {
                return (List<ThUnderGroundParking>)gridControl_parking.DataSource;
            }

            set
            {
                gridControl_parking.DataSource = value;
            }
        }

        public void Reload()
        {
            //
        }

        public void InitializeGridControl()
        {
            Presenter = new ThUnderGroundParkingPresenter(this);
            DbRepository = new ThUnderGroundParkingDbRepository();
            if (DbRepository.Parkings.Count == 0)
            {
                DbRepository.AppendUnderGroundParking();
            }
            gridControl_parking.DataSource = DbRepository.Parkings;
            gridControl_parking.RefreshDataSource();
        }
    }
}
