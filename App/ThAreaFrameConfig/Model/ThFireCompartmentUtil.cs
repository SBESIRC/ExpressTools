using System;
using System.Text.RegularExpressions;

namespace ThAreaFrameConfig.Model
{
    public static class ThFireCompartmentUtil
    {
        private static string AreaTextContent(this IntPtr frame)
        {
            return string.Format("面积：{0:0.00}m\u00B2", frame.AreaEx());
        }

        public static string CommerceTextContent(this IntPtr frame, UInt16 subKey, UInt16 storey, UInt16 index)
        {
            return CommerceSerialNumber(subKey, storey, index) + "\\P" + AreaTextContent(frame);
        }

        public static string CommerceSerialNumber(UInt16 subKey, UInt16 storey, UInt16 index)
        {
            return string.Format("FC-{0}-FL{1}-{2}", subKey, storey, index);
        }

        public static string UpdateCommerceSerialNumber(this string contents, UInt16 subKey, UInt16 storey, UInt16 index)
        {
            string[] tokens = Regex.Split(contents, @"\\P");
            tokens[0] = CommerceSerialNumber(subKey, storey, index);
            return tokens[0] + "\\P" + tokens[1];
        }

        public static bool MatchCommerceSerialNumber(this string sn, ref UInt16 subKey, ref UInt16 storey, ref UInt16 index)
        {
            Match match = Regex.Match(sn, @"^FC-([0-9]+)-FL([0-9]+)-([0-9]+)$");
            if (!match.Success)
            {
                return false;
            }

            subKey = UInt16.Parse(match.Groups[1].Value);
            storey = UInt16.Parse(match.Groups[2].Value);
            index = UInt16.Parse(match.Groups[3].Value);
            return true;
        }
    }
}
