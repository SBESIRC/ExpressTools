using System;
using System.Linq;
using System.Collections.Generic;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrameConfig.Model
{
    public class ThFireCompartmentUtil
    {
        private static string CommerceSerialNumber(ThFireCompartment compartment, string subKey)
        {
            return string.Format("FC-{0}-FL{1}-{2}", subKey, compartment.Storey, compartment.Number);
        }

        private static string UndergroundParkingSerialNumber(ThFireCompartment compartment)
        {
            return string.Format("防火分区{0}", Tools.NumberToChinese(compartment.Number));
        }

        private static string AreaTextContent(ThFireCompartment compartment)
        {
            return string.Format("面积：{0:2}m\u00B2", compartment.Area);
        }

        public static string CommerceTextContent(ThFireCompartment compartment, string subKey)
        {
            return CommerceSerialNumber(compartment, subKey) + "\\P" + AreaTextContent(compartment);
        }

        public static string UndergroundParkingTextContent(ThFireCompartment compartment)
        {
            return UndergroundParkingSerialNumber(compartment) + "\\P" + AreaTextContent(compartment);
        }
    }
}
