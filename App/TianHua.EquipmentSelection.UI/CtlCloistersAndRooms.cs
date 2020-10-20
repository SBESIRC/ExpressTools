﻿using System;
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
    public partial class CtlCloistersAndRooms : CtlExhaustControlBase
    {
        FanDataModel m_Fan { get; set; }
        Action panelchanged;

        public CtlCloistersAndRooms()
        {
            InitializeComponent();
        }

        public override void InitForm(FanDataModel _FanDataModel,Action action)
        {
            m_Fan = _FanDataModel;
            panelchanged = action;
            this.RadSpray.SelectedIndex = m_Fan.ExhaustModel.IsSpray ? 0 : 1;
            this.TxtHeight.Text = m_Fan.ExhaustModel.SpaceHeight;
            this.TxtArea.Text = m_Fan.ExhaustModel.CoveredArea;
            this.TxtMinUnitVolume.Text = m_Fan.ExhaustModel.MinAirVolume;
            m_Fan.ExhaustModel.UnitVolume = this.TxtUnitVolume.Text;
            m_Fan.ExhaustModel.SpatialTypes = "办公室、学校、客厅、走道";
            m_Fan.ExhaustModel.MinAirVolume = this.TxtMinUnitVolume.Text;
        }

        private void RadSpraySelectedChanged(object sender, EventArgs e)
        {
            m_Fan.ExhaustModel.IsSpray = this.RadSpray.SelectedIndex == 0 ? true : false;
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
            TxtMinUnitVolume.Text = fmExhaustCalculator.GetMinVolumeForCtlCloistersRooms(m_Fan.ExhaustModel).ToString();
            m_Fan.ExhaustModel.MinAirVolume = TxtMinUnitVolume.Text;
        }

    }
}