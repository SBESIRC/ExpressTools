using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThFireCompartmentAreaFrame
    {
        // 面积框线
        public IntPtr Frame { get; set; }

        // 内框孤岛框线
        public List<IntPtr> IslandFrames { get; set; }

        // 有效疏散宽度
        public List<IntPtr> EvacuationWidthNotes { get; set; }

        // 安全出口
        public List<IntPtr> EmergencyExitNotes { get; set; }

        // 面积
        public double Area
        {
            get
            {
                double area = Frame.Area();
                IslandFrames.ForEach(o => area -= o.Area());
                return area;
            }
        }

        // 疏散密度
        public double EvacuationDensity
        {
            get
            {
                if (EvacuationWidthNotes.Count == 0)
                {
                    return 0.0;
                }

                return EvacuationWidthNotes.Sum(o => o.EvacuationWidth());
            }
        }

        // 安全出口
        public UInt16 EmergencyExit
        {
            get
            {
                if (EmergencyExitNotes.Count == 0)
                {
                    return 0;
                }

                return EmergencyExitNotes.Max(o => o.EmergencyExit());
            }
        }

        // 状态
        public bool IsDefined
        {
            get
            {
                return Frame != (IntPtr)0;
            }
        }
    }
}
