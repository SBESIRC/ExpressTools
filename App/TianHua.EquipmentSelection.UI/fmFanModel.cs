using System;
using System.IO;
using System.Linq;
using System.Data;
using System.Drawing;
using DevExpress.XtraEditors;
using TianHua.Publics.BaseCode;
using System.Collections.Generic;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TianHua.FanSelection.UI
{
    public partial class fmFanModel : DevExpress.XtraEditors.XtraForm
    {
        public FanDataModel m_Fan = new FanDataModel();

        public List<FanDataModel> m_ListFan = new List<FanDataModel>();

        public List<AxialFanEfficiency> m_ListAxialFanEfficiency = new List<AxialFanEfficiency>();

        public List<FanEfficiency> m_ListFanEfficiency = new List<FanEfficiency>();

        public List<MotorPower> m_ListMotorPower = new List<MotorPower>();

        public fmFanModel()
        {
            InitializeComponent();


            InitData();
        }

        private void fmFanModel_Load(object sender, EventArgs e)
        {
            this.Size = new Size(438, 440);

        }

        public void InitData()
        {
            var _JsonFanEfficiency = ReadTxt(Path.Combine(ThCADCommon.SupportPath(), ThFanSelectionCommon.HTFC_Efficiency));
            m_ListFanEfficiency = FuncJson.Deserialize<List<FanEfficiency>>(_JsonFanEfficiency);


            var _JsonAxialFanEfficiency = ReadTxt(Path.Combine(ThCADCommon.SupportPath(), ThFanSelectionCommon.AXIAL_Efficiency));
            m_ListAxialFanEfficiency = FuncJson.Deserialize<List<AxialFanEfficiency>>(_JsonAxialFanEfficiency);

            var _JsonMotorPower = ReadTxt(Path.Combine(ThCADCommon.SupportPath(), ThFanSelectionCommon.MOTOR_POWER));
            m_ListMotorPower = FuncJson.Deserialize<List<MotorPower>>(_JsonMotorPower);


            if (m_ListFanEfficiency != null && m_ListFanEfficiency.Count > 0)
            {
                m_ListFanEfficiency.ForEach(p =>
                {
                    if (p.No_Max == string.Empty) p.No_Max = "999";
                    if (p.No_Min == string.Empty) p.No_Max = "0";
                    if (p.Rpm_Max == string.Empty) p.Rpm_Max = "999";
                    if (p.Rpm_Min == string.Empty) p.Rpm_Min = "0";
                });
            }

            if (m_ListAxialFanEfficiency != null && m_ListAxialFanEfficiency.Count > 0)
            {
                m_ListAxialFanEfficiency.ForEach(p =>
                {
                    if (p.No_Max == string.Empty) p.No_Max = "999";
                    if (p.No_Min == string.Empty) p.No_Max = "0";
                });
            }


        }

        public void InitForm(FanDataModel _FanDataModel, List<FanDataModel> _ListFan)
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
            LabAir.Text = FuncStr.NullToStr(_FanDataModel.AirVolume);
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
                if (_List != null && _List.Count > 0)
                {
                    RGroupFanControl.EditValue = "双速";

                    RGroupFanControl.Enabled = false;
                }
                else
                {
                    RGroupFanControl.Enabled = true;
                }

            }

            CalcFanEfficiency(_FanDataModel);

        }

        private void CalcFanEfficiency(FanDataModel _FanDataModel)
        {
            //比转速	等于5.54*风机转速（查询）*比转数下的流量的0.5次方 /全压输入值的0.75次方		
            //轴功率    风量乘以全压除以风机内效率除以传动效率（0.855）除以1000					
            //电机容量安全系数 =IF(AZ6<=0.5,1.5, IF(AZ6<=1,1.4,IF(AZ6<=2,1.3,IF(AZ6<=5,1.2,IF(AZ6<=20,1.15,1.1)))))
            if (_FanDataModel.VentStyle == "轴流")
            {
                double _SafetyFactor = 0;
                var _SpecificSpeed = 5.54 * FuncStr.NullToDouble(_FanDataModel.FanModelFanSpeed) * Math.Pow(_FanDataModel.AirVolume / 3600, 0.5) / Math.Pow(_FanDataModel.WindResis, 0.75);

                var _AxialFanEfficiency = m_ListAxialFanEfficiency.Find(p => FuncStr.NullToInt(p.No_Min) < FuncStr.NullToInt(_FanDataModel.FanModelNum) && FuncStr.NullToInt(p.No_Max) > FuncStr.NullToInt(_FanDataModel.FanModelNum)
                   && _FanDataModel.VentLev == p.FanEfficiencyLevel);

                var _ShaftPower = _FanDataModel.AirVolume * _FanDataModel.WindResis / _AxialFanEfficiency.FanEfficiency / 100 / 0.855 / 1000;
                if (_ShaftPower <= 1.5)
                {
                    _SafetyFactor = 1.5;
                }
                else if (_ShaftPower <= 1)
                {
                    _SafetyFactor = 1.4;
                }
                else if (_ShaftPower <= 2)
                {
                    _SafetyFactor = 1.3;
                }
                else if (_ShaftPower <= 5)
                {
                    _SafetyFactor = 1.2;
                }
                else if (_ShaftPower <= 5)
                {
                    _SafetyFactor = 1.15;
                }
                else
                {
                    _SafetyFactor = 1.1;
                }
                var _MotorEfficiency = PubVar.g_ListMotorEfficiency.Find(p => p.Key == _FanDataModel.VentConnect);
                var _Tmp = _ShaftPower / 0.85;
                var _ListMotorPower = m_ListMotorPower.FindAll(p => FuncStr.NullToDouble(p.Power) >= _Tmp && p.MotorEfficiencyLevel == _FanDataModel.EleLev && p.Rpm == FuncStr.NullToStr(_FanDataModel.MotorTempo));
                var _MotorPower = _ListMotorPower.OrderBy(p => FuncStr.NullToDouble(p.Power)).First();

                var _EstimatedMotorPower = _Tmp / FuncStr.NullToDouble(_MotorPower.MotorEfficiency) / FuncStr.NullToDouble(_MotorEfficiency.Value) * _SafetyFactor;
                _ListMotorPower = m_ListMotorPower.FindAll(p => FuncStr.NullToDouble(p.Power) >= _EstimatedMotorPower && p.MotorEfficiencyLevel == _FanDataModel.EleLev && p.Rpm == FuncStr.NullToStr(_FanDataModel.MotorTempo));
                _MotorPower = _ListMotorPower.OrderBy(p => FuncStr.NullToDouble(p.Power)).First();

                if (_MotorPower != null)
                {
                    LabMotorPower.Text = _MotorPower.Power;
                    _FanDataModel.FanModelMotorPower = _MotorPower.Power;
                    _FanDataModel.FanInternalEfficiency = FuncStr.NullToStr(_AxialFanEfficiency.FanEfficiency);
                }



                var _FanModelPower = _FanDataModel.WindResis / 3600 * _AxialFanEfficiency.FanEfficiency * 0.855 * 0.98;
                _FanDataModel.FanModelPower = FuncStr.NullToStr(_FanModelPower);
                LabPower.Text = _FanDataModel.FanModelPower;

            }
            else
            {
                double _SafetyFactor = 0;
                var _SpecificSpeed = 5.54 * FuncStr.NullToDouble(_FanDataModel.FanModelFanSpeed) * Math.Pow(_FanDataModel.AirVolume / 3600, 0.5) / Math.Pow(_FanDataModel.WindResis, 0.75);

                var _FanEfficiency = m_ListFanEfficiency.Find(p => FuncStr.NullToInt(p.No_Min) < FuncStr.NullToInt(_FanDataModel.FanModelNum) && FuncStr.NullToInt(p.No_Max) > FuncStr.NullToInt(_FanDataModel.FanModelNum)
                     && FuncStr.NullToInt(p.Rpm_Min) < FuncStr.NullToInt(_SpecificSpeed)
                      && FuncStr.NullToInt(p.Rpm_Max) > FuncStr.NullToInt(_SpecificSpeed) && _FanDataModel.VentLev == p.FanEfficiencyLevel);

                var _ShaftPower = _FanDataModel.AirVolume * _FanDataModel.WindResis / _FanEfficiency.FanInternalEfficiency / 100 / 0.855 / 1000;
                if (_ShaftPower <= 1.5)
                {
                    _SafetyFactor = 1.5;
                }
                else if (_ShaftPower <= 1)
                {
                    _SafetyFactor = 1.4;
                }
                else if (_ShaftPower <= 2)
                {
                    _SafetyFactor = 1.3;
                }
                else if (_ShaftPower <= 5)
                {
                    _SafetyFactor = 1.2;
                }
                else if (_ShaftPower <= 5)
                {
                    _SafetyFactor = 1.15;
                }
                else
                {
                    _SafetyFactor = 1.1;
                }

                var _MotorEfficiency = PubVar.g_ListMotorEfficiency.Find(p => p.Key == _FanDataModel.VentConnect);
                var _Tmp = _ShaftPower / 0.85;
                var _ListMotorPower = m_ListMotorPower.FindAll(p => FuncStr.NullToDouble(p.Power) >= _Tmp && p.MotorEfficiencyLevel == _FanDataModel.EleLev && p.Rpm == FuncStr.NullToStr(_FanDataModel.MotorTempo));
                var _MotorPower = _ListMotorPower.OrderBy(p => FuncStr.NullToDouble(p.Power)).First();

                var _EstimatedMotorPower = _Tmp / FuncStr.NullToDouble(_MotorPower.MotorEfficiency) / FuncStr.NullToDouble(_MotorEfficiency.Value) * _SafetyFactor;
                _ListMotorPower = m_ListMotorPower.FindAll(p => FuncStr.NullToDouble(p.Power) >= _EstimatedMotorPower && p.MotorEfficiencyLevel == _FanDataModel.EleLev && p.Rpm == FuncStr.NullToStr(_FanDataModel.MotorTempo));
                _MotorPower = _ListMotorPower.OrderBy(p => FuncStr.NullToDouble(p.Power)).First();

                if (_MotorPower != null)
                {
                    LabMotorPower.Text = _MotorPower.Power;
                    _FanDataModel.FanModelMotorPower = _MotorPower.Power;
                    _FanDataModel.FanInternalEfficiency = FuncStr.NullToStr( _FanEfficiency.FanInternalEfficiency);
                }


                var _FanModelPower = _FanDataModel.WindResis / 3600 * _FanEfficiency.FanInternalEfficiency * 0.855 * 0.98;
                _FanDataModel.FanModelPower = FuncStr.NullToStr( _FanModelPower);
                LabPower.Text = _FanDataModel.FanModelPower;

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
            if (FuncStr.NullToStr(RGroupFanControl.EditValue) == "单速")
            {
                CheckFrequency.Enabled = true;
            }
            else
            {
                CheckFrequency.Checked = false;
                CheckFrequency.Enabled = false;
            }

        }


        public string ReadTxt(string _Path)
        {
            try
            {
                using (StreamReader _StreamReader = File.OpenText(_Path))
                {
                    return _StreamReader.ReadToEnd();
                }
            }
            catch
            {
                XtraMessageBox.Show("数据文件读取时发生错误！");
                return string.Empty;

            }
        }
    }
}
