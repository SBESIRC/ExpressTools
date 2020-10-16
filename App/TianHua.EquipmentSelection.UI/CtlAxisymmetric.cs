using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianHua.FanSelection.UI.CtlExhaustCalculation;
using TianHua.FanSelection.Model;
using TianHua.Publics.BaseCode;

namespace TianHua.FanSelection.UI
{
    public partial class CtlAxisymmetric : CtlExhaustControlBase
    {
        FanDataModel m_Fan { get; set; }
        Action panelchanged;

        public CtlAxisymmetric()
        {
            InitializeComponent();
        }

        public override void InitForm(FanDataModel _FanDataModel,Action action)
        {
            m_Fan = _FanDataModel;
            panelchanged = action;
            this.textEdit1.Text = m_Fan.ExhaustModel.Axial_HighestHeight;
            this.textEdit2.Text = m_Fan.ExhaustModel.Axial_HangingWallGround;
            this.textEdit3.Text = m_Fan.ExhaustModel.Axial_FuelFloor;
            this.textEdit4.Text = m_Fan.ExhaustModel.Axial_CalcAirVolum;
            m_Fan.ExhaustModel.Final_CalcAirVolum = m_Fan.ExhaustModel.Axial_CalcAirVolum;
        }

        private void textEdit1ValueChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.Axial_HighestHeight = this.textEdit1.Text;
            UpdateCalcAirVolum(m_Fan.ExhaustModel);
        }

        private void textEdit2ValueChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.Axial_HangingWallGround = this.textEdit2.Text;
            UpdateCalcAirVolum(m_Fan.ExhaustModel);
        }

        private void textEdit3ValueChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.Axial_FuelFloor = this.textEdit3.Text;
            UpdateCalcAirVolum(m_Fan.ExhaustModel);
        }

        public override void UpdateCalcAirVolum(ExhaustCalcModel model)
        {
            if (m_Fan == null)
            {
                return;
            }
            this.textEdit4.Text = fmExhaustCalculator.GetCalcAirVolum(model).ToString();
            m_Fan.ExhaustModel.Axial_CalcAirVolum = this.textEdit4.Text;
            m_Fan.ExhaustModel.Final_CalcAirVolum = m_Fan.ExhaustModel.Axial_CalcAirVolum;

            if (model.SpaceHeight.NullToDouble() < 3)
            {
                this.textEdit1.ReadOnly = true;
            }
            else
            {
                this.textEdit1.ReadOnly = false;
            }
            panelchanged();
        }
    }
}
