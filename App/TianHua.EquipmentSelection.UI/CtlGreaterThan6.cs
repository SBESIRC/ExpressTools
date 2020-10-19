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
    public partial class CtlGreaterThan6 : CtlExhaustControlBase
    {
        FanDataModel m_Fan { get; set; }
        Action panelchanged;

        public CtlGreaterThan6()
        {
            InitializeComponent();
        }

        public override void InitForm(FanDataModel _FanDataModel,Action action)
        {
            m_Fan = _FanDataModel;
            panelchanged = action;
            this.RadSpray.SelectedIndex = m_Fan.ExhaustModel.IsSpray ? 0 : 1;
            this.ComBoxSpatialType.Text = m_Fan.ExhaustModel.SpatialTypes;
            this.TxtHeight.Text = m_Fan.ExhaustModel.SpaceHeight;
            this.TxtMinUnitVolume.Text = m_Fan.ExhaustModel.MinAirVolume;
        }

        private void SpatialTypeSelectedChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.SpatialTypes = ComBoxSpatialType.Text;
            TxtMinUnitVolume.Text = fmExhaustCalculator.GetMinVolumeForGreater6(m_Fan.ExhaustModel).ToString();
            panelchanged();
        }

        private void TxtHeightChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.SpaceHeight = TxtHeight.Text;
            this.TxtMinUnitVolume.Text = fmExhaustCalculator.GetMinVolumeForGreater6(m_Fan.ExhaustModel).ToString();
            m_Fan.ExhaustModel.MinAirVolume = this.TxtMinUnitVolume.Text;
            panelchanged();
        }

        private void RadSpraySelectedChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.IsSpray = this.RadSpray.SelectedIndex == 0 ? true : false;
            this.TxtMinUnitVolume.Text = fmExhaustCalculator.GetMinVolumeForGreater6(m_Fan.ExhaustModel).ToString();
            m_Fan.ExhaustModel.MinAirVolume = this.TxtMinUnitVolume.Text;
            panelchanged();
        }

    }
}
