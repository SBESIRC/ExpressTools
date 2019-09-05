using System;
using System.Linq;
using System.Collections.Generic;
using ThAreaFrameConfig.View;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraTab;
using Autodesk.AutoCAD.Runtime;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThResidentialBuildingControl : DevExpress.XtraEditors.XtraUserControl, IResidentialBuildingView
    {
        private ThResidentialBuildingPresenter Presenter;
        private ThResidentialBuildingDbRepository DbRepository;

        public ThResidentialBuildingControl()
        {
            InitializeComponent();
            InitializePresenter();
            InitializeDataSource();
            InitializeDataBinding();
        }

        private void InitializePresenter()
        {
            Presenter = new ThResidentialBuildingPresenter(this);
        }

        public void InitializeDataBinding()
        {
            textEdit_number.DataBindings.Add("EditValue", DbRepository.Building, "Number", true);
            textEdit_name.DataBindings.Add("EditValue", DbRepository.Building, "Name", true);
            comboBoxEdit_category.DataBindings.Add("EditValue", DbRepository.Building, "Category", true);
            textEdit_above_ground_storeys.DataBindings.Add("EditValue", DbRepository.Building, "AboveGroundFloorNumber", true);
            textEdit_under_ground_storeys.DataBindings.Add("EditValue", DbRepository.Building, "UnderGroundFloorNumber", true);
        }

        public void InitializeDataSource()
        {
            DbRepository = new ThResidentialBuildingDbRepository();
        }

        public void Reload()
        {
            InitializeDataSource();
        }

        private void simpleButton_OK_Click(object sender, EventArgs e)
        {
            string name = ThResidentialRoomUtil.LayerName(DbRepository.Building);
            Presenter.OnPickAreaFrames(name);
            DbRepository.Building.Layer = name;
        }

        private void simpleButton_modify_Click(object sender, EventArgs e)
        {
            string name = DbRepository.Building.Layer;
            string newName = ThResidentialRoomUtil.LayerName(DbRepository.Building);
            Presenter.OnRenameAreaFrameLayer(name, newName);
            DbRepository.Building.Layer = newName;
        }
    }
}
