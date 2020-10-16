using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianHua.FanSelection.Model;
using TianHua.FanSelection.UI.CtlExhaustCalculation;

namespace TianHua.FanSelection.UI
{
    public partial class CtlBalconyOverFlow : CtlExhaustControlBase
    {
        FanDataModel m_Fan { get; set; }
        Action panelchanged;

        public CtlBalconyOverFlow()
        {
            InitializeComponent();
        }

        public override void InitForm(FanDataModel _FanDataModel,Action action)
        {
            m_Fan = _FanDataModel;
            panelchanged = action;
            this.TxtFuelToBalcony.Text = m_Fan.ExhaustModel.Spill_FuelBalcony;
            this.TxtBalconySmokeBottom.Text = m_Fan.ExhaustModel.Spill_BalconySmokeBottom;
            this.TxtFireOpening.Text = m_Fan.ExhaustModel.Spill_FireOpening;
            this.TxtOpenBalcony.Text = m_Fan.ExhaustModel.Spill_OpenBalcony;
        }

        private void FuelToBalconyChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.Spill_FuelBalcony = this.TxtFuelToBalcony.Text;
            UpdateCalcAirVolum(m_Fan.ExhaustModel);
        }

        private void BalconySmokeBottomChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.Spill_BalconySmokeBottom = this.TxtBalconySmokeBottom.Text;
            UpdateCalcAirVolum(m_Fan.ExhaustModel);
        }

        private void FireOpeningChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.Spill_FireOpening = this.TxtFireOpening.Text;
            UpdateCalcAirVolum(m_Fan.ExhaustModel);
        }

        private void OpenBalconyChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.Spill_OpenBalcony = this.TxtOpenBalcony.Text;
            UpdateCalcAirVolum(m_Fan.ExhaustModel);
        }

        public override void UpdateCalcAirVolum(ExhaustCalcModel model)
        {
            if (m_Fan == null)
            {
                return;
            }
            this.TxtCalculateVolume.Text = fmExhaustCalculator.GetCalcAirVolum(model).ToString();
            m_Fan.ExhaustModel.Spill_CalcAirVolum = this.TxtCalculateVolume.Text;
            m_Fan.ExhaustModel.Final_CalcAirVolum = m_Fan.ExhaustModel.Spill_CalcAirVolum;
            panelchanged();
        }

    }
}
