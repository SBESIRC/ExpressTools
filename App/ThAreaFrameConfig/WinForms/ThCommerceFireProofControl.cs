using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using DevExpress.XtraEditors;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Presenter;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThCommerceFireProofControl : XtraUserControl, IFCCommerceView
    {
        private ThFCCommerceNullRepository DbRepository;
        //private ThFCCommerceDbRepository DbRepository;

        public ThCommerceFireProofControl()
        {
            InitializeComponent();
            InitializeGridControl();
            InitializeDataBindings();
        }

        public ThCommerceFireProofSettings Settings
        {
            get
            {
                return DbRepository.Settings;
            }
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
            DbRepository = new ThFCCommerceNullRepository();
            DbRepository.AppendDefaultFireCompartment();
            gridControl_fire_compartment.DataSource = DbRepository.Settings.Compartments;
            gridControl_fire_compartment.RefreshDataSource();
        }

        public void InitializeGridControl()
        {
            DbRepository = new ThFCCommerceNullRepository();
            DbRepository.AppendDefaultFireCompartment();
            gridControl_fire_compartment.DataSource = DbRepository.Settings.Compartments;
            gridControl_fire_compartment.RefreshDataSource();
        }

        public void InitializeDataBindings()
        {
            // 子项编号
            textBox_sub_key.DataBindings.Add(
                "Text", 
                DbRepository,
                "Settings.SubKey", 
                true, 
                DataSourceUpdateMode.OnPropertyChanged);

            // 地上层数
            textBox_storey.DataBindings.Add(
                "Text",
                DbRepository,
                "Settings.Storey",
                true,
                DataSourceUpdateMode.OnPropertyChanged);
        }
    }
}
