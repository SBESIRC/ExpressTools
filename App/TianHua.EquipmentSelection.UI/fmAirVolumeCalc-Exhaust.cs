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
    public partial class fmAirVolumeCalc_Exhaust : DevExpress.XtraEditors.XtraForm
    {
        public FanDataModel m_Fan { get; set; }

        public fmAirVolumeCalc_Exhaust()
        {
            InitializeComponent();
        }

        public void InitForm(FanDataModel _FanDataModel)
        {
            var _Json = FuncJson.Serialize(_FanDataModel);

            m_Fan = FuncJson.Deserialize<FanDataModel>(_Json);

            if (m_Fan.ExhaustModel.IsNull())
            {
                m_Fan.AirCalcFactor = 1.2;
                TxtFactor.Text = FuncStr.NullToStr(m_Fan.AirCalcFactor);
                return;
            }

            this.TxtCalcValue.Text = FuncStr.NullToStr(Math.Max(m_Fan.ExhaustModel.Final_CalcAirVolum.NullToDouble(), m_Fan.ExhaustModel.MinAirVolume.NullToDouble()));
            TxtEstimatedValue.Text = FuncStr.NullToStr(m_Fan.AirCalcValue);
            TxtFactor.Text = FuncStr.NullToStr(m_Fan.AirCalcFactor);
            TxtAirVolume.Text = FuncStr.NullToStr(m_Fan.AirVolume);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {

        }

        private void TxtCalcValue_Click(object sender, EventArgs e)
        {
            fmScenario _fmScenario = new fmScenario();
            _fmScenario.InitForm(m_Fan);
            if (_fmScenario.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            m_Fan.ExhaustModel = _fmScenario.m_Fan.ExhaustModel;
            if (!m_Fan.ExhaustModel.IsNull())
            {
                this.TxtCalcValue.Text = FuncStr.NullToStr(Math.Max(m_Fan.ExhaustModel.Final_CalcAirVolum.NullToDouble(), m_Fan.ExhaustModel.MinAirVolume.NullToDouble()));
            }
        }

        private void TxtCalcValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;

        }

        private void TxtAirVolume_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;


        }

        private void fmAirVolumeCalc_Exhaust_Load(object sender, EventArgs e)
        {
       
        }

        private void CalculateAirVolumeChanged(object sender, EventArgs e)
        {
            m_Fan.AirVolume = this.TxtAirVolume.Text.NullToInt();
        }

        private void EstimateValueChanged(object sender, EventArgs e)
        {
            m_Fan.AirCalcValue = this.TxtEstimatedValue.Text.NullToInt();
            UpdateAirVolume();
        }

        private void SelectFactorChanged(object sender, EventArgs e)
        {
            m_Fan.AirCalcFactor = this.TxtFactor.Text.NullToDouble();
            UpdateAirVolume();
        }

        private void CalculateValueChanged(object sender, EventArgs e)
        {
            UpdateAirVolume();
        }

        private void UpdateAirVolume()
        {
            this.TxtAirVolume.Text = FuncStr.NullToStr(Math.Round(Math.Max(this.TxtCalcValue.Text.NullToDouble(), this.TxtEstimatedValue.Text.NullToDouble()) * this.TxtFactor.Text.NullToDouble()));
            m_Fan.AirVolume = this.TxtAirVolume.Text.NullToInt();
        }

        private void FactorChangedCheck(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                this.TxtFactor.Text = fmExhaustCalculator.SelectionFactorCheck(this.TxtFactor.Text);
            }
        }

        private void FactorLeaveCheck(object sender, EventArgs e)
        {
            this.TxtFactor.Text = fmExhaustCalculator.SelectionFactorCheck(this.TxtFactor.Text);
        }
    }
}
