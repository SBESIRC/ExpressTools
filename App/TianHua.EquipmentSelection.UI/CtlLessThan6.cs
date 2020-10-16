using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.XtraEditors;
using TianHua.Publics.BaseCode;
using TianHua.FanSelection.UI.CtlExhaustCalculation;

namespace TianHua.FanSelection.UI
{
    public partial class CtlLessThan6 : CtlExhaustControlBase
    {
        FanDataModel m_Fan { get; set; }
        Action panelchanged;

        public CtlLessThan6()
        {
            InitializeComponent();
        }

        public override void InitForm(FanDataModel _FanDataModel,Action action)
        {
            m_Fan = _FanDataModel;
            panelchanged = action;
            this.ComBoxSpatialType.Text = m_Fan.ExhaustModel.SpatialTypes;
            this.TxtHeight.Text = m_Fan.ExhaustModel.SpaceHeight;
            this.TxtArea.Text = m_Fan.ExhaustModel.CoveredArea;
            m_Fan.ExhaustModel.UnitVolume = this.TxtUnitVolume.Text;
            this.TxtMinUnitVolume.Text = m_Fan.ExhaustModel.MinAirVolume;
        }

        private void SpatialTypeChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.SpatialTypes = ComBoxSpatialType.Text;
            panelchanged();
        }

        private void TxtHeightChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.SpaceHeight = TxtHeight.Text;
            panelchanged();
        }

        private void TxtAreaChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.CoveredArea = TxtArea.Text;
            UpdateMinAirVolume();
        }

        private void UpdateMinAirVolume()
        {
            TxtMinUnitVolume.Text = fmExhaustCalculator.GetMinVolumeForLess6(m_Fan.ExhaustModel).ToString();
            m_Fan.ExhaustModel.MinAirVolume = TxtMinUnitVolume.Text;
        }
    }
}
