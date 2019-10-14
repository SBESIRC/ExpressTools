using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrame
{
    public class ThAreaFrameOutdoorParkingSpace
    {
        // 车场车位
        public string type;

        // 室外车场
        public string entity;

        // 车场类型
        public string category;

        // 停车类型
        public string parkingCategory;

        // 车场层数
        public string multiple;

        // 地坪标高
        public string elevation;

        // 版本号
        public string version;

        // 图层名
        public string layer;

        // 构造函数
        public ThAreaFrameOutdoorParkingSpace(string name)
        {
            layer = name;
        }

        public static ThAreaFrameOutdoorParkingSpace Space(string name)
        {
            string[] tokens = name.Split('_');
            if (tokens.Last() == ThCADCommon.RegAppName_AreaFrame_Version_Legacy)
            {
                ThAreaFrameOutdoorParkingSpace space = new ThAreaFrameOutdoorParkingSpace(name)
                {
                    type = tokens[0],
                    entity = tokens[1],
                    category = tokens[2],
                    parkingCategory = tokens[3],
                    multiple = tokens[4],
                    elevation = "",
                    version = tokens[5]
                };
                return space;
            }
            else if (tokens.Last() == ThCADCommon.RegAppName_AreaFrame_Version)
            {
                ThAreaFrameOutdoorParkingSpace space = new ThAreaFrameOutdoorParkingSpace(name)
                {
                    type = tokens[0],
                    entity = tokens[1],
                    category = tokens[2],
                    parkingCategory = tokens[3],
                    multiple = tokens[4],
                    elevation = tokens[5],
                    version = tokens[6]
                };
                return space;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
