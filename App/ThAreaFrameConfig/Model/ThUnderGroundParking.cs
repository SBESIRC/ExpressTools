using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThUnderGroundParking
    {
        // ID
        [Display(AutoGenerateField = false)]
        public Guid ID { get; set; }

        // 序号
        public int Number { get; set; }

        // 停车类型
        public string Category {
            get
            {
                return "小型汽车";
            }
        }

        // 车场层数
        public int Floors { get; set; }

        // 所属层
        public string Storey { get; set; }

        // 室内车位数
        public int Slots {
            get
            {
                return Floors * Frames.Count;
            }
        }

        // 状态
        public bool IsDefined
        {
            get
            {
                return Frames.Count > 0;
            }
        }

        // 面积框线
        public List<IntPtr> Frames { get; set; }
    }
}
