using System;
using System.ComponentModel.DataAnnotations;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThAOccupancy
    {
        private int? floors;
        private string category;

        // ID
        [Display(AutoGenerateField = false)]
        public Guid ID { get; set; }

        // 楼层ID
        [Display(AutoGenerateField = false)]
        public Guid StoreyID { get; set; }

        // 编号
        public int Number { get; set; }

        // 构件
        public string Component { get; set; }

        // 类型
        public string Category {
            get
            {
                return category;
            }

            set
            {
                category = value;
                if (category == "室内停车库")
                {
                    floors = 1;
                }
                else
                {
                    floors = null;
                }
            }

        }

        // 计算系数
        public double Coefficient { get; set; }

        // 计容系数
        public double FARCoefficient { get; set; }

        // 车位层数
        public int? Floors {
            get
            {
                return floors;
            }

            set
            {
                floors = value;
            }
        }

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
