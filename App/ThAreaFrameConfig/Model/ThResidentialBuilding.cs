using System;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThResidentialBuilding
    {
        // 建筑编号
        public string Number { get; set; }

        // 建筑名称
        public string Name { get; set; }

        // 单体性质
        public string Category { get; set; }

        // 地上层数
        public string AboveGroundFloorNumber { get; set; }

        // 地下层数
        public string UnderGroundFloorNumber { get; set; }
    }
}
