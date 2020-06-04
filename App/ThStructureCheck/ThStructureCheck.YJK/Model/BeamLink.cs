﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class BeamLink
    {
        public BeamStatus Status { get; set; }
        public List<YjkEntityInfo> Start { get; set; }
        public List<YjkEntityInfo> End { get; set; }
        public List<CalcBeamSeg> Beams { get; set; }
    }
    public enum BeamStatus
    {
        Primary,
        Secondary,
        Unknown
    }
}
