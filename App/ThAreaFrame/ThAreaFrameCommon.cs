using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrame
{
    public class ThAreaFrameCommon
    {
        // 公摊
        public string type;

        // 计算系数
        public string areaFactor;

        // 公摊名称
        public string name;

        // 所在层
        public string storeys;

        // 公摊标识
        public string publicAreaID;

        // 分摊此公摊的套内标识
        public string dwellingID;

        // 版本号
        public string version;

        // 图层名
        public string layer;

        // 构造函数
        public ThAreaFrameCommon(string name)
        {
            layer = name;
        }

        public static ThAreaFrameCommon Common(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameCommon common = new ThAreaFrameCommon(name)
            {
                type = tokens[0],
                areaFactor = tokens[1],
                name = tokens[2],
                storeys = tokens[3],
                publicAreaID = tokens[4],
                dwellingID = tokens[5],
                version = tokens[6]

            };
            return common;
        }
    }
}
