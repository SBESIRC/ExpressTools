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

namespace TianHua.FanSelection.UI
{
    public partial class CtlGarage : CtlExhaustControlBase
    {
        FanDataModel m_Fan { get; set; }
        Action panelchanged;

        public CtlGarage()
        {
            InitializeComponent();
        }

        public override void InitForm(FanDataModel _FanDataModel,Action action)
        {
            m_Fan = _FanDataModel;
            panelchanged = action;
            this.RadSpray.SelectedIndex = m_Fan.ExhaustModel.IsSpray ? 0 : 1;
            m_Fan.ExhaustModel.SpatialTypes = "汽车库";
            this.TxtHeight.Text = m_Fan.ExhaustModel.SpaceHeight;
            this.TxtMinUnitVolume.Text = m_Fan.ExhaustModel.MinAirVolume;
        }

        private void TxtHeightChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.SpaceHeight = TxtHeight.Text;
            this.TxtMinUnitVolume.Text = fmExhaustCalculator.GetMinVolumeForGarage(m_Fan.ExhaustModel).ToString();
            m_Fan.ExhaustModel.MinAirVolume = this.TxtMinUnitVolume.Text;
            panelchanged();
        }

        private void RadSpraySelectedChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.IsSpray = this.RadSpray.SelectedIndex == 0 ? true : false;
            panelchanged();
        }

    }
}
