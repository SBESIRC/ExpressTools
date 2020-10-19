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

namespace TianHua.FanSelection.UI
{
    public partial class CtlWindowOpen : CtlExhaustControlBase
    {
        public FanDataModel m_Fan { get; set; }
        Action panelchanged;

        public CtlWindowOpen()
        {
            InitializeComponent();
        }

        public override void InitForm(FanDataModel _FanDataModel,Action action)
        {
            m_Fan = _FanDataModel;
            panelchanged = action;
            this.TxtWindowArea.Text = m_Fan.ExhaustModel.Window_WindowArea;
            this.TxtWindowHeight.Text = m_Fan.ExhaustModel.Window_WindowHeight;
            this.TxtSmokeToBottom.Text = m_Fan.ExhaustModel.Window_SmokeBottom;
        }

        private void WindowAreaChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.Window_WindowArea = this.TxtWindowArea.Text;
            UpdateCalcAirVolum(m_Fan.ExhaustModel);
        }

        private void WindowHeightChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.Window_WindowHeight = this.TxtWindowHeight.Text;
            UpdateCalcAirVolum(m_Fan.ExhaustModel);
        }

        private void SmokeToBottomChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.Window_SmokeBottom = this.TxtSmokeToBottom.Text;
            UpdateCalcAirVolum(m_Fan.ExhaustModel);
        }

        public override void UpdateCalcAirVolum(ExhaustCalcModel model)
        {
            if (m_Fan == null)
            {
                return;
            }
            this.TxtCalculateVolume.Text = fmExhaustCalculator.GetCalcAirVolum(model);
            m_Fan.ExhaustModel.Window_CalcAirVolum = this.TxtCalculateVolume.Text;
            m_Fan.ExhaustModel.Final_CalcAirVolum = m_Fan.ExhaustModel.Window_CalcAirVolum;
            panelchanged();
        }
    }
}
