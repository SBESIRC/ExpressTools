using System;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.Model
{
    public class ThFCCommerceSettings : ThFireCompartmentSettings
    {
        public enum OccupantDensity
        {
            Low = 0,
            Middle = 1,
            High = 2
        }
        public enum FireResistance
        {
            Level1 = 0,
            Level2 = 1,
            Level3 = 2,
            Level4 = 3
        }

        // 建筑信息
        public UInt16 SubKey { get; set; }
        public UInt16 Storey { get; set; }
        public OccupantDensity Density { get; set; }
        public FireResistance Resistance { get; set; }
    }
}
