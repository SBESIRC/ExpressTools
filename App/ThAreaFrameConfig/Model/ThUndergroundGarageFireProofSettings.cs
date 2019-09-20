using System;
using System.Collections.Generic;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.ViewModel
{
    public class ThUndergroundGarageFireProofSettings
    {
        // 图层
        public Dictionary<string, string> Layers { get; set; }

        // 防火分区
        public List<ThFireCompartment> Compartments { get; set; }

        // 是否生成防火分区填充
        public bool GenerateHatch { get; set; }
    }
}
