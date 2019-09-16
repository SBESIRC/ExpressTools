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
        public string Name { get; set; }
        public string Coefficient { get; set; }
        public string FARCoefficient { get; set; }
        public string Area { get; set; }
        public string Pick { get; set; }

        // 面积框线
        public List<ThResidentialAreaFrame> AreaFrames { get; set; }

        private static ThResidentialRoomComponent ConstructComponent(string name, Guid guid)
        {
            Guid componentGuid = Guid.NewGuid();
            ThResidentialRoomComponent component = new ThResidentialRoomComponent()
            {
                ID = componentGuid,
                Name = name,
                Coefficient = "计算系数",
                FARCoefficient = "计容系数",
                Area = "面积（m2）",
                Pick = "选择",
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
