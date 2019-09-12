using System;
using System.Collections.Generic;
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
        public int Storey { get; set; }

        // 车位数
        public int Slot
        {
            get
            {
                return Storey * Frames.Count;
            }
        }

        // 面积框线
        public List<IntPtr> Frames { get; set; }

        // 状态
        public bool IsDefined
        {
            get
            {
                return (Frames.Count > 0);
            }
        }
    }
}
