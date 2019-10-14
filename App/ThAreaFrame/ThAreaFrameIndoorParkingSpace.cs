using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThAreaFrame
{
    public class ThAreaFrameIndoorParkingSpace
    {
        // 单体车位
        public string type;

        // 停车类型
        public string parkingCategory;

        // 车场层数
        public string multiple;

        // 所属层
        public string storey;

        // 版本号
        public string version;

        // 图层名
        public string layer;

        // 构造函数
        public ThAreaFrameIndoorParkingSpace(string name)
        {
            layer = name;
        }

        public static ThAreaFrameIndoorParkingSpace Space(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameIndoorParkingSpace space = new ThAreaFrameIndoorParkingSpace(name)
            {
                type = tokens[0],
                parkingCategory = tokens[1],
                multiple = tokens[2],
                storey = tokens[3],
                version = tokens[4]
            };
            return space;
        }
    }
}
