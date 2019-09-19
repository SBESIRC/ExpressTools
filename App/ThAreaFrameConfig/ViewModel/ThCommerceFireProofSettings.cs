using System;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.ViewModel
{
    public class ThCommerceFireProofSettings
    {
        public enum Density { Low, Middle, High }
        public enum FireResistance { Level1, Level2, Level3, Level4 }
        public struct BuildingInfo
        {
            public UInt16 subKey;
            public FireResistance fireResistance;
            public UInt16 AboveGroundStoreys;
        };

        // 建筑信息
        public BuildingInfo Info { get; set; }

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
