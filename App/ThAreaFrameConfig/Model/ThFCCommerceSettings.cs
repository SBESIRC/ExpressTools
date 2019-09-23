using System;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.Model
{
    public class ThFCCommerceSettings
    {
        public enum Density { Low, Middle, High }
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
        public FireResistance Resistance { get; set; }

        // 图层
        public Dictionary<string, string> Layers { get; set; }

        // 防火分区
        public List<ThFireCompartment> Compartments { get; set; }

        // 是否生成防火分区填充
        public bool GenerateHatch { get; set; }

        // 人员密度
        public Density PfValue { get; set; }
    }
}
