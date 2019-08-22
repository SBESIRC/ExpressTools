using System;
using System.ComponentModel.DataAnnotations;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThOutdoorParkingSpace
    {
        // ID
        [Display(AutoGenerateField = false)]
        public Guid ID { get; set; }

        // 编号
        public int Number { get; set; }

        // 面积
        public double Area
        {
            get
            {
                return Frame.Area();
            }
        }

        // 车场类型
        public string Category {
            get
            {
                return "露天车场";
            }
        }

        // 停车类型
        public string ParkingCategory
        {
            get
            {
                return "小型汽车";
            }
        }

        // 车场/车位层数
        public UInt16 Storey { get; set; }

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
}
