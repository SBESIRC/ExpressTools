using System;
using System.ComponentModel.DataAnnotations;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    class ThPublicGreenSpace
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
