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
        // ID
        [Display(AutoGenerateField = false)]
        public Guid ID { get; set; }

        // 子项编号
        public UInt16 Subkey { get; set; }

        // 序号
        public UInt16 Number { get; set; }

        // 楼层
        public UInt16 Storey { get; set; }

        // 编号
        public string Identifier { get; set; }

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

        // 自动灭火系统
        public bool SelfExtinguishingSystem { get; set; }

        // 面积框线
        public List<ThFireCompartmentAreaFrame> Frames;
    }
}
