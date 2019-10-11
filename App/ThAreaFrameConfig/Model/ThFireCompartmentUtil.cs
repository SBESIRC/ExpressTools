using System;
using System.Text.RegularExpressions;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrameConfig.Model
{
    public static class ThFireCompartmentUtil
    {
        private static string AreaTextContent(this ThFireCompartment compartment)
        {
            return string.Format("面积：{0:0.00}m\u00B2", compartment.Area);
        }

        public static string CommerceTextContent(this ThFireCompartment compartment)
        {
            if (compartment.Type == ThFireCompartment.FCType.FCCommerce)
            {
                return CommerceSerialNumber(compartment.Subkey, compartment.Storey, compartment.Index) 
                    + "\\P"
                    + compartment.AreaTextContent();
            }
            else if (compartment.Type == ThFireCompartment.FCType.FCUnderGroundParking)
            {
                return UndergroundParkingSerialNumber(compartment.Index)
                    + "\\P"
                    + compartment.AreaTextContent();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static string CommerceSerialNumber(UInt16 subKey, Int16 storey, UInt16 index)
        {
            return string.Format("FC-{0}-FL{1}-{2}", subKey, storey, index);
        }

        public static string UndergroundParkingSerialNumber(UInt16 index)
        {
            int value = (int)index;
            return string.Format("防火分区{0}", value.NumberToChinese());
        }

        public static bool MatchCommerceSerialNumber(this string sn, ref UInt16 subKey, ref Int16 storey, ref UInt16 index)
        {
            Match match = Regex.Match(sn, @"^FC-([0-9]+)-FL(-?[0-9]+)-([0-9]+)$");
            if (!match.Success)
            {
                return false;
            }

            subKey = UInt16.Parse(match.Groups[1].Value);
            storey = Int16.Parse(match.Groups[2].Value);
            index = UInt16.Parse(match.Groups[3].Value);
            return true;
        }

        public static bool MatchUndergroundParkingSerialNumber(this string sn, ref UInt16 index)
        {
            if (!sn.StartsWith("防火分区"))
            {
                return false;
            }

            string subNumber = sn.SubNumber();
            if (subNumber.IsNullOrEmpty())
            {
                return false;
            }

            index = Convert.ToUInt16(subNumber.ChineseToNumber());
            return true;
        }

        public static double EvacuationWidth(this string note)
        {
            Match match = Regex.Match(note, @"^有效疏散宽度([0-9]+)m$");
            if (!match.Success)
            {
                return 0.0;
            }

            return double.Parse(match.Groups[1].Value);
        }

        public static double EvacuationDistance(this string note)
        {
            Match match = Regex.Match(note, @"^最远疏散距离([0-9]*(?:\.[0-9]*)?)m$");
            if (!match.Success)
            {
                return 0.0;
            }

            return double.Parse(match.Groups[1].Value);
        }

        public static UInt16 EmergencyExit(this string note)
        {
            Match match = Regex.Match(note, @"^安全出口([0-9]+)$");
            if (!match.Success)
            {
                return 0;
            }

            return UInt16.Parse(match.Groups[1].Value);
        }
    }
}
