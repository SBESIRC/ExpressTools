using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrame
{
    public class ThAreaFrameRoofGreenSpace
    {
        // 屋顶构件
        public string type;

        // 屋顶绿地
        public string areaType;

        // 折算系数
        public string coefficient;

        // 版本号
        public string version;

        // 图层名
        public string layer;

        // 构造函数
        public ThAreaFrameRoofGreenSpace(string name)
        {
            layer = name;
        } 

        public static ThAreaFrameRoofGreenSpace RoofOfGreenSpace(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameRoofGreenSpace roofGreenSpace = new ThAreaFrameRoofGreenSpace(name)
            {
                type = tokens[0],
                areaType = tokens[1],
                coefficient = tokens[2],
                version = tokens[3]
            };
            return roofGreenSpace;
        }
    }
}
