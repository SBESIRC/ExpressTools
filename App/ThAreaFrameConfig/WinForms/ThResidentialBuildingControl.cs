using System;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThResidentialBuildingControl : DevExpress.XtraEditors.XtraUserControl
    {
        private ThResidentialBuildingDbRepository DbRepository;

        public ThResidentialBuildingControl()
        {
            InitializeComponent();
            InitializeDataSource();
            InitializeDataBinding();
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

        private void simpleButton_OK_Click(object sender, EventArgs e)
        {
            //
        }

        private void simpleButton_modify_Click(object sender, EventArgs e)
        {
            //
        }
    }
}
