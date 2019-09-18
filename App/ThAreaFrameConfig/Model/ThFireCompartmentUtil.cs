using System;
using System.Linq;
using System.Collections.Generic;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrameConfig.Model
{
    public class ThFireCompartmentUtil
    {
        public static string CommerceSerialNumber(ThFireCompartment compartment, string subKey)
        {
            return string.Format("FC-{0}-FL{1}-{2}", subKey, compartment.Storey, compartment.Number);
        }

        public static string UndergroundParkingSerialNumber(ThFireCompartment compartment)
        {
            return string.Format("防火分区{0}", Tools.NumberToChinese(compartment.Number));
        }

        public static string AreaTextContent(ThFireCompartment compartment)
        {
            return string.Format("面积：{0}m2", compartment.Area);
        }
    }
}
