using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrame
{
    public class ThAreaFrameAOccupancy
    {
        // 附属公建
        public string type;

        // 
        public string areaType;

        // 类型
        public string occupancyType;

        // 构件名称
        public string component;

        // 性质
        public string category;

        // 计算系数
        public string areaRatio;

        // 计容系数
        public string floorAreaRatio;

        // 车位层数
        public string parkingStoreys;

        // 所属层
        public string storeys;

        // 公摊标识
        public string publicAreaID;

        // 版本号
        public string version;

        // 图层名
        public string layer;

        public ThAreaFrameAOccupancy(string name)
        {
            layer = name;
        }

        // 主体
        public static ThAreaFrameAOccupancy Main(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameAOccupancy main = new ThAreaFrameAOccupancy(name)
            {
                type                = tokens[0],
                areaType            = tokens[1],
                occupancyType       = tokens[2],
                category            = tokens[3],
                areaRatio           = tokens[4],
                floorAreaRatio      = tokens[5],
                parkingStoreys      = tokens[6],
                storeys             = tokens[7],
                publicAreaID        = tokens[8],
                version             = tokens[9]
            };
            return main;
        }

        // 阳台
        public static ThAreaFrameAOccupancy Balcony(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameAOccupancy balcony = new ThAreaFrameAOccupancy(name)
            {
                type                = tokens[0],
                areaType            = tokens[1],
                occupancyType       = tokens[2],
                category            = tokens[3],
                areaRatio           = tokens[4],
                floorAreaRatio      = tokens[5],
                storeys             = tokens[6],
                publicAreaID        = tokens[7],
                version             = tokens[8]

            };
            return balcony;
        }

        // 架空
        public static ThAreaFrameAOccupancy Stilt(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameAOccupancy open = new ThAreaFrameAOccupancy(name)
            {
                type                = tokens[0],
                areaType            = tokens[1],
                occupancyType       = tokens[2],
                category            = tokens[3],
                areaRatio           = tokens[4],
                floorAreaRatio      = tokens[5],
                parkingStoreys      = tokens[6],
                storeys             = tokens[7],
                version             = tokens[8]

            };
            return open;
        }

        // 飘窗
        public static ThAreaFrameAOccupancy Baywindow(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameAOccupancy baywindow = new ThAreaFrameAOccupancy(name)
            {
                type                = tokens[0],
                areaType            = tokens[1],
                occupancyType       = tokens[2],
                category            = tokens[3],
                areaRatio           = tokens[4],
                floorAreaRatio      = tokens[5],
                storeys             = tokens[6],
                version             = tokens[7]

            };
            return baywindow;
        }

        // 雨棚
        public static ThAreaFrameAOccupancy Rainshed(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameAOccupancy rainshed = new ThAreaFrameAOccupancy(name)
            {
                type                = tokens[0],
                areaType            = tokens[1],
                occupancyType       = tokens[2],
                category            = tokens[3],
                areaRatio           = tokens[4],
                floorAreaRatio      = tokens[5],
                storeys             = tokens[6],
                version             = tokens[7]

            };
            return rainshed;
        }

        // 附属其他构件
        public static ThAreaFrameAOccupancy Miscellaneous(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameAOccupancy miscellaneous = new ThAreaFrameAOccupancy(name)
            {
                type                = tokens[0],
                areaType            = tokens[1],
                component           = tokens[2],
                occupancyType       = tokens[3],
                category            = tokens[4],
                areaRatio           = tokens[5],
                floorAreaRatio      = tokens[6],
                storeys             = tokens[7],
                version             = tokens[8]

            };
            return miscellaneous;
        }
    }
}
