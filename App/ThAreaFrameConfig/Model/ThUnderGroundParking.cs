using System;
using Linq2Acad;
using System.ComponentModel.DataAnnotations;
using Autodesk.AutoCAD.DatabaseServices;
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
        public UInt16 Floors { get; set; }

        // 所属层
        public string Storey { get; set; }

        // 室内车位数
        public UInt16 Slots {
            get
            {
                return 0;
            }
        }
    }
}
