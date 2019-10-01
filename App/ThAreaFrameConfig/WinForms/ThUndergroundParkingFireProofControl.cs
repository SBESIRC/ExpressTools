using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Presenter;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using Autodesk.AutoCAD.Runtime;
using AcHelper;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThUndergroundParkingFireProofControl : XtraUserControl, IFireCompartmentView
    {
        private ThFCUndergroundParkingPresenter Presenter;
        private ThFCUnderGroundParkingDbRepository DbRepository;


        public ThUndergroundParkingFireProofControl()
        {
            InitializeComponent();
            InitializeGridControl();
            InitializeDataBindings();
        }

        public ThFCUnderGroundParkingSettings Settings
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
            throw new NotImplementedException();
        }

        public void InitializeGridControl()
        {
            Presenter = new ThFCUndergroundParkingPresenter(this);
            DbRepository = new ThFCUnderGroundParkingDbRepository();
            DbRepository.AppendDefaultFireCompartment();
            gridControl_fire_compartment.DataSource = Settings.Compartments;
            gridControl_fire_compartment.RefreshDataSource();
            label_merge_compartment.Text = MergedCompartmentCountString();
        }

        private string MergedCompartmentCountString()
        {
            int count = Settings.Compartments.Where(o => o.IsMerged).Count();
            return string.Format("点击合并防火分区：已建立合并关系{0}个", count);
        }

        public void InitializeDataBindings()
        {
            // 指定外框线图层
            comboBox_outer_frame.DataSource = DbRepository.Layers;
            comboBox_outer_frame.SelectedItem = Settings.Layers["OUTERFRAME"];

            // 指定内框线图层
            comboBox_inner_frame.DataSource = DbRepository.Layers;
            comboBox_inner_frame.SelectedItem = Settings.Layers["INNERFRAME"];
        }
    }
}
