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
    public partial class CtlSmokeExtraction : CtlExhaustControlBase
    {
        FanDataModel m_Fan { get; set; }
        Action panelchanged;

        public CtlSmokeExtraction()
        {
            InitializeComponent();
        }

        public override void InitForm(FanDataModel _FanDataModel,Action action)
        {
            m_Fan = _FanDataModel;
            panelchanged = action;
            this.simpleLabelItem1.Text = m_Fan.ExhaustModel.ExhaustCalcType;
            if (m_Fan.ExhaustModel.ExhaustCalcType == "中庭-周围场所设有排烟系统")
            {
                this.TxtMinUnitVolume.Text = "107000";
                this.Notetext.Text = ThFanSelectionUICommon.NOTE_CENTER_EXTRACTION;
            }
            else
            {
                this.TxtMinUnitVolume.Text = "40000";
                this.Notetext.Text = ThFanSelectionUICommon.NOTE_CENTER_EXTRACTION_NOSMOKE;
            }
            m_Fan.ExhaustModel.MinAirVolume = this.TxtMinUnitVolume.Text;
            this.RadSpray.SelectedIndex = m_Fan.ExhaustModel.IsSpray ? 0 : 1;
            this.ComBoxSpatialType.Text = m_Fan.ExhaustModel.SpatialTypes;
            this.TxtHeight.Text = m_Fan.ExhaustModel.SpaceHeight;
        }

        private void SpatialTypeSelectedChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.SpatialTypes = ComBoxSpatialType.Text;
            panelchanged();
        }

        private void TxtHeightChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.SpaceHeight = TxtHeight.Text;
            panelchanged();
        }

        private void RadSpraySelectedChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.IsSpray = this.RadSpray.SelectedIndex == 0 ? true : false;
            panelchanged();
        }

    }
}
