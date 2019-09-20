using System;
using System.Linq;
using System.Collections.Generic;
using TianHua.AutoCAD.Utility.ExtensionTools;

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
    }
}
