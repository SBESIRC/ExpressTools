using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Model
{
    public class ThFireCompartmentSettings
    {
        // 图层
        public Dictionary<string, string> Layers { get; set; }

        // 防火分区
        public List<ThFireCompartment> Compartments { get; set; }
    }
}
