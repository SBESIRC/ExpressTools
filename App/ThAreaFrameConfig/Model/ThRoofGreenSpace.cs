using System;
using System.ComponentModel.DataAnnotations;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThRoofGreenSpace
    {
        // ID
        [Display(AutoGenerateField = false)]
        public Guid ID { get; set; }

        // 序号
        public int Number { get; set; }

        // 折算系数
        public double Coefficient { get; set; }

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
    }
}
