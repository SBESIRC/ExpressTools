using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianHua.Publics.BaseCode;

namespace TianHua.FanSelection.UI
{
    public partial class fmFanModel : DevExpress.XtraEditors.XtraForm
    {
        public FanDataModel m_Fan = new FanDataModel();

        public List<FanDataModel> m_ListFan = new List<FanDataModel>();

        public fmFanModel()
        {
            InitializeComponent();
        }

        private void fmFanModel_Load(object sender, EventArgs e)
        {
            this.Size = new Size(438, 440);
        }


        public void InitForm(FanDataModel _FanDataModel,List<FanDataModel> _ListFan)
        {
            m_Fan = _FanDataModel;
            m_ListFan = _ListFan;
            if (FuncStr.NullToStr(_FanDataModel.VentStyle) == "轴流")
            {
                layouLX.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                layouZL.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            }
            else
            {
                layouZL.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                layouLX.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            }


            LabModelNum.Text = _FanDataModel.FanModelNum;
            LabCCFC.Text = _FanDataModel.FanModelCCCF;
            LabAir.Text = FuncStr.NullToStr( _FanDataModel.AirVolume);
            LabPa.Text = FuncStr.NullToStr(_FanDataModel.WindResis);
            LabMotorPower.Text = _FanDataModel.FanModelMotorPower;
            LabNoise.Text = _FanDataModel.FanModelNoise;
            LabFanSpeed.Text = _FanDataModel.FanModelFanSpeed;
            LabPower.Text = _FanDataModel.FanModelPower;

            LabLength.Text = _FanDataModel.FanModelLength;
            LabWidth.Text = _FanDataModel.FanModelWidth;
            LabHeight.Text = _FanDataModel.FanModelHeight;
            LabWeight.Text = _FanDataModel.FanModelWeight;


            LabZLLength.Text = _FanDataModel.FanModelLength;
            LabZLWeight.Text = _FanDataModel.FanModelWeight;
            LabDIA.Text = _FanDataModel.FanModelDIA;

            RGroupFanControl.EditValue = _FanDataModel.Control;
            CheckFrequency.EditValue = _FanDataModel.IsFre;
            RGroupPower.EditValue = _FanDataModel.PowerType;

            if (FuncStr.NullToStr(_FanDataModel.Scenario).Contains("消防") || FuncStr.NullToStr(_FanDataModel.Scenario).Contains("事故"))
            {
                RGroupPower.Enabled = false;
            }
            else
            {
                RGroupPower.Enabled = true;
            }
     


            if (_FanDataModel.Scenario == "平时送风" || _FanDataModel.Scenario == "平时排风")
            {
                var _List = _ListFan.FindAll(p => p.PID == _FanDataModel.ID);
                if(_List != null && _List.Count > 0)
                {
                    RGroupFanControl.EditValue = "双速";

                    RGroupFanControl.Enabled = false;
                }
                else
                {
                    RGroupFanControl.Enabled = true;
                }

            }

        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            m_Fan.FanModelNum = FuncStr.NullToStr(LabModelNum.Text);
            m_Fan.FanModelCCCF = FuncStr.NullToStr(LabCCFC.Text);


            m_Fan.AirVolume = FuncStr.NullToInt(LabAir.Text);

            m_Fan.WindResis = FuncStr.NullToInt(LabPa.Text);

            m_Fan.FanModelMotorPower = FuncStr.NullToStr(LabMotorPower.Text);
            m_Fan.FanModelNoise = FuncStr.NullToStr(LabNoise.Text);
            m_Fan.FanModelFanSpeed = FuncStr.NullToStr(LabFanSpeed.Text);


            m_Fan.FanModelPower = FuncStr.NullToStr(LabPower.Text);

            m_Fan.FanModelLength = FuncStr.NullToStr(LabLength.Text);
            m_Fan.FanModelWidth = FuncStr.NullToStr(LabWidth.Text);
            m_Fan.FanModelHeight = FuncStr.NullToStr(LabHeight.Text);
            m_Fan.FanModelWeight = FuncStr.NullToStr(LabWeight.Text);


            m_Fan.Control = FuncStr.NullToStr(RGroupFanControl.EditValue);

            m_Fan.IsFre = CheckFrequency.Checked;
            m_Fan.PowerType = FuncStr.NullToStr(RGroupPower.EditValue);


        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {

        }

        private void RGroupFanControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(FuncStr.NullToStr(RGroupFanControl.EditValue) == "单速")
            {
                CheckFrequency.Enabled = true;
            }
            else
            {
                CheckFrequency.Checked = false;
                CheckFrequency.Enabled = false;
            }

        }
    }
}
