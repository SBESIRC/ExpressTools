using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using DevExpress.XtraEditors;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Presenter;
using System.Collections.Generic;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThCommerceFireProofControl : XtraUserControl, IFCCommerceView
    {
        //private ThFCCommerceDbRepository DbRepository;

        public ThCommerceFireProofControl()
        {
            InitializeComponent();
            InitializeGridControl();
            InitializeDataBindings();
        }

        public List<ThFireCompartment> Compartments
        {
            get
            {
                return (List<ThFireCompartment>)gridControl_fire_compartment.DataSource;
            }

            set
            {
                gridControl_fire_compartment.DataSource = value;
            }
        }

        public void Reload()
        {
            /*
            DbRepository = new ThFCCommerceDbRepository();
            DbRepository.AppendDefaultFireCompartment();
            gridControl_fire_compartment.DataSource = DbRepository.Settings.Compartments;
            gridControl_fire_compartment.RefreshDataSource();
            */
        }

        public void InitializeGridControl()
        {
            /*
            DbRepository = new ThFCCommerceDbRepository();
            DbRepository.AppendDefaultFireCompartment();
            gridControl_fire_compartment.DataSource = DbRepository.Settings.Compartments;
            gridControl_fire_compartment.RefreshDataSource();
            */
        }

        public void InitializeDataBindings()
        {
            //
        }
    }
}
