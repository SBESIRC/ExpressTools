using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Linq2Acad;

namespace ThAreaFrameConfig.Model
{
    public class ThFireCompartmentAreaFrame
    {
        // 面积
        public double Area
        {
            get
            {
                return Frame.AreaEx();
            }
        }

        // 面积框线
        public IntPtr Frame { get; set; }

        // 状态
        public bool IsDefined
        {
            get
            {
                return Frame != (IntPtr)0;
            }
        }
    }

    public class ThFireCompartment
    {
        // 子项编号
        public UInt16 Subkey { get; set; }

        // 楼层
        public UInt16 Storey { get; set; }

        // 序号
        public UInt16 Index { get; set; }

        // 自动灭火系统
        public bool SelfExtinguishingSystem { get; set; }

        // 面积框线
        public List<ThFireCompartmentAreaFrame> Frames;

        // 编号
        public string Identifier {
            get
            {
                return ThFireCompartmentUtil.CommerceSerialNumber(Subkey, Storey, Index);
            }
        }

        // 面积
        public double Area
        {
            get
            {
                double area = 0.0;
                Frames.Where(o => o.IsDefined)
                    .ToList()
                    .ForEach(o => area += o.Area);
                return area;
            }
        }

        // 是否合并
        public bool IsMerged
        {
            get
            {
                return Frames.Where(o => o.IsDefined).Count() > 1;
            }
        }
    }
}
