using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrame
{
    public class ThAreaFrameRoof
    {
        // 单体楼顶间
        public string type;
        // 所属类型
        public string category;
        // 计算系数
        public string areaRatio;
        // 计容系数
        public string floorAreaRatio;
        // 版本号
        public string version;
        // 图层名
        public string layer;

        public ThAreaFrameRoof (string name)
        {
            layer = name;
        }

        public static ThAreaFrameRoof Roof(string name)
        {
            string[] tokens = name.Split('_');
            if (tokens.Count() == 4)
            {
                ThAreaFrameRoof roof = new ThAreaFrameRoof(name)
                {
                    type = tokens[0],
                    category = "住宅",
                    areaRatio = tokens[1],
                    floorAreaRatio = tokens[2],
                    version = tokens[3]
                };
                return roof;
            }
            else if (tokens.Count() == 5)
            {
                ThAreaFrameRoof roof = new ThAreaFrameRoof(name)
                {
                    type = tokens[0],
                    category = tokens[1],
                    areaRatio = tokens[2],
                    floorAreaRatio = tokens[3],
                    version = tokens[4]
                };
                return roof;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
