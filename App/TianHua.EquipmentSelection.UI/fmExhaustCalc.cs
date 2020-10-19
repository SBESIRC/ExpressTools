using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianHua.FanSelection.UI.CtlExhaustCalculation;
using TianHua.Publics.BaseCode;

namespace TianHua.FanSelection.UI
{
    public partial class fmExhaustCalc : DevExpress.XtraEditors.XtraForm
    {
        public FanDataModel m_Fan { get; set; }
        public event Action panel2change;
        public event Action panel1change;
        public fmExhaustCalc()
        {
            InitializeComponent();
        }

        public void InitForm(FanDataModel _FanDataModel, string type)
        {
            var _Json = FuncJson.Serialize(_FanDataModel);
            m_Fan = FuncJson.Deserialize<FanDataModel>(_Json);

            panel2change += TotalUpdate;
            panel1change += UpdateVmax;
            m_Fan.ExhaustModel.ExhaustCalcType = type;
            InitsidePanel2(type, panel2change);
            InitsidePanel1(m_Fan.ExhaustModel.PlumeSelection, panel1change);
            if (type == "空间-净高小于等于6m")
            {
                this.TxtHRR.Enabled = false;
                this.ComBoxPlume.Enabled = false;
            }
            this.TxtHRR.Text = fmExhaustCalculator.GetHeatReleaseRate(m_Fan.ExhaustModel).ToString();
            m_Fan.ExhaustModel.HeatReleaseRate = this.TxtHRR.Text;

            this.ComBoxPlume.SelectedIndex = 0;
            m_Fan.ExhaustModel.PlumeSelection = this.ComBoxPlume.Text;

            this.TxtLength.Text = m_Fan.ExhaustModel.SmokeLength;
            this.TxtWidth.Text = m_Fan.ExhaustModel.SmokeWidth;
            this.TxtDiameter.Text = m_Fan.ExhaustModel.SmokeDiameter;
            this.TxtSmokePosition.Text = m_Fan.ExhaustModel.SmokeFactorValue;
            this.ComBoxWZ.SelectedIndex = 0;
            m_Fan.ExhaustModel.SmokeFactorOption = this.ComBoxWZ.Text;
            this.TxtSmokeLayerThickness.Text = m_Fan.ExhaustModel.SmokeThickness;
            this.TxtVmax.Text = fmExhaustCalculator.GetMaxSmoke(m_Fan.ExhaustModel).ToString();
            switch (type)
            {
                case "空间-净高小于等于6m":
                    this.Height = 740;
                    break;
                case "空间-净高大于6m":
                    this.Height = 720;
                    break;
                case "空间-汽车库":
                    this.Height = 690;
                    break;
                case "走道回廊-仅走道或回廊设置排烟":
                    this.Height = 710;
                    break;
                case "走道回廊-房间内和走道或回廊都设置排烟":
                    this.Height = 780;
                    break;
                case "中庭-周围场所设有排烟系统":
                case "中庭-周围场所不设排烟系统":
                    this.Height = 780;
                    break;
                default:
                    break;
            }
        }

        public void InitsidePanel2(string type,Action action)
        {
            this.sidePanel2.Controls.Clear();
            CtlExhaustControlBase sidePanelcontrol = new CtlExhaustControlBase();
            switch (type)
            {
                case "空间-净高小于等于6m":
                    sidePanelcontrol = new CtlLessThan6();
                    break;
                case "空间-净高大于6m":
                    sidePanelcontrol = new CtlGreaterThan6();
                    break;
                case "空间-汽车库":
                    sidePanelcontrol = new CtlGarage();
                    break;
                case "走道回廊-仅走道或回廊设置排烟":
                    sidePanelcontrol = new CtlCloister();
                    break;
                case "走道回廊-房间内和走道或回廊都设置排烟":
                    sidePanelcontrol = new CtlCloistersAndRooms();
                    break;
                case "中庭-周围场所设有排烟系统":
                case "中庭-周围场所不设排烟系统":
                    sidePanelcontrol = new CtlSmokeExtraction();
                    break;
                default:
                    break;
            }
            sidePanelcontrol.InitForm(m_Fan, action);
            this.sidePanel2.Controls.Add(sidePanelcontrol);
            sidePanelcontrol.Dock = DockStyle.Fill;

        }

        public void InitsidePanel1(string Plume, Action action)
        {
            this.sidePanel1.Controls.Clear();
            CtlExhaustControlBase sidePanelcontrol = new CtlExhaustControlBase();
            switch (Plume)
            {
                case "轴对称型":
                    sidePanelcontrol = new CtlAxisymmetric();
                    break;
                case "阳台溢出型":
                    sidePanelcontrol = new CtlBalconyOverFlow();
                    break;
                case "窗口型":
                    sidePanelcontrol = new CtlWindowOpen();
                    break;
                default:
                    sidePanelcontrol = new CtlAxisymmetric();
                    break;
            }
            sidePanelcontrol.InitForm(m_Fan,action);
            this.sidePanel1.Controls.Add(sidePanelcontrol);
            sidePanelcontrol.Dock = DockStyle.Fill;
            if (m_Fan.ExhaustModel.ExhaustCalcType == "空间-净高小于等于6m")
            {
                sidePanelcontrol.Enabled = false;
            }
        }

        private void TxtHRRChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.HeatReleaseRate = TxtHRR.Text;
        }

        private void sidePanel2_Leave(object sender, EventArgs e)
        {
            TotalUpdate();
        }

        private void PlumeSelectedChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.PlumeSelection = this.ComBoxPlume.Text;
            InitsidePanel1(this.ComBoxPlume.Text, panel1change);
            TotalUpdate();
        }

        private void TxtLengthValueChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.SmokeLength = this.TxtLength.Text;
            m_Fan.ExhaustModel.SmokeDiameter = fmExhaustCalculator.GetSmokeDiameter(m_Fan.ExhaustModel).ToString();
            this.TxtDiameter.Text = m_Fan.ExhaustModel.SmokeDiameter;
        }

        private void TxtWidthValueChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.SmokeWidth = this.TxtWidth.Text;
            m_Fan.ExhaustModel.SmokeDiameter = fmExhaustCalculator.GetSmokeDiameter(m_Fan.ExhaustModel).ToString();
            this.TxtDiameter.Text = m_Fan.ExhaustModel.SmokeDiameter;
        }

        private void SmokeFactorOptionSelectChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.SmokeFactorOption = this.ComBoxWZ.Text;
            m_Fan.ExhaustModel.SmokeFactorValue = fmExhaustCalculator.GetSmokeFactor(m_Fan.ExhaustModel).ToString();
            this.TxtSmokePosition.Text = fmExhaustCalculator.GetSmokeFactor(m_Fan.ExhaustModel).ToString();
        }

        private void SmokeLayerThicknessChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.SmokeThickness = this.TxtSmokeLayerThickness.Text;
            this.TxtVmax.Text = fmExhaustCalculator.GetMaxSmoke(m_Fan.ExhaustModel).ToString();
            m_Fan.ExhaustModel.MaxSmokeExtraction = this.TxtVmax.Text;
        }

        private void SmokePositionChanged(object sender, EventArgs e)
        {
            this.TxtVmax.Text = fmExhaustCalculator.GetMaxSmoke(m_Fan.ExhaustModel).ToString();
            m_Fan.ExhaustModel.MaxSmokeExtraction = this.TxtVmax.Text;
        }

        private void sidePanel1_Leave(object sender, EventArgs e)
        {
            TotalUpdate();
        }

        private void TotalUpdate()
        {
            this.TxtHRR.Text = fmExhaustCalculator.GetHeatReleaseRate(m_Fan.ExhaustModel).ToString();
            m_Fan.ExhaustModel.HeatReleaseRate = this.TxtHRR.Text;

            CtlExhaustControlBase sidePanelcontrol = sidePanel1.Controls[0] as CtlExhaustControlBase;
            sidePanelcontrol.UpdateCalcAirVolum(m_Fan.ExhaustModel);

            UpdateVmax();
        }

        private void UpdateVmax()
        {
            this.TxtHRR.Text = fmExhaustCalculator.GetHeatReleaseRate(m_Fan.ExhaustModel).ToString();
            m_Fan.ExhaustModel.HeatReleaseRate = this.TxtHRR.Text;

            this.TxtVmax.Text = fmExhaustCalculator.GetMaxSmoke(m_Fan.ExhaustModel).ToString();
            m_Fan.ExhaustModel.MaxSmokeExtraction = this.TxtVmax.Text;
        }
    }
}
