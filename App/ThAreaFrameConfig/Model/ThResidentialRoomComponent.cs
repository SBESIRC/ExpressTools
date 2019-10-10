using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThResidentialRoomComponent
    {
        // ID
        [Display(AutoGenerateField = false)]
        public Guid ID { get; set; }

        // 标题
        private readonly string name;
        public string Name
        {
            get
            {
                return name;
            }
        }
        public string Coefficient
        {
            get
            {
                return "计算系数";
            }
        }
        public string FARCoefficient
        {
            get
            {
                return "计容系数";
            }
        }
        public string Area
        {
            get
            {
                return "面积（m\u00B2）";
            }
        }
        public string Pick
        {
            get
            {
                return "";
            }
        }

        // 构造函数
        public ThResidentialRoomComponent(string theName)
        {
            name = theName;
        }

        // 面积框线
        public List<ThResidentialAreaFrame> AreaFrames { get; set; }

        private static ThResidentialRoomComponent ConstructComponent(string name, Guid guid)
        {
            Guid componentGuid = Guid.NewGuid();
            ThResidentialRoomComponent component = new ThResidentialRoomComponent(name)
            {
                ID = componentGuid,
                AreaFrames = new List<ThResidentialAreaFrame>()
            };
            return component;
        }

        public static ThResidentialRoomComponent Dwelling(Guid roomID)
        {
            return ConstructComponent("套内", roomID);
        }

        public static ThResidentialRoomComponent Balcony(Guid roomID)
        {
            return ConstructComponent("阳台", roomID);
        }

        public static ThResidentialRoomComponent Baywindow(Guid roomID)
        {
            return ConstructComponent("飘窗", roomID);
        }

        public static ThResidentialRoomComponent Miscellaneous(Guid roomID)
        {
            return ConstructComponent("其他构件", roomID);
        }
    }
}
