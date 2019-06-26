using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ThAreaFrame
{
    // 单体基础信息
    public class ThAreaFrameFoundation : IComparable<ThAreaFrameFoundation>
    {
        // 单体基底
        public string type;

        // 建筑编号
        public string numberID;

        // 建筑名称
        public string name;

        // 单体性质
        public string category;

        // 地上层数
        public string aboveGroundFloorNumber;

        // 地下层数
        public string underGroundFloorNumber;

        // 层高
        public string storeyHeight;

        // 地下室露高
        public string dewHeight;

        // 室内外高差
        public string elevationDifference;

        // 屋面高度
        public string roofHeight;

        // 是否计入面积计算
        public string useInArea;

        // 概况
        public string description;

        // 版本号
        public string version;

        // 图层名
        public string layer;

        public ThAreaFrameFoundation(string name)
        {
            layer = name;
        }

        public static ThAreaFrameFoundation Foundation(string name)
        {
            string[] tokens = name.Split('_');
            ThAreaFrameFoundation foundation = new ThAreaFrameFoundation(name)
            {
                type = tokens[0],
                numberID = tokens[1],
                name = tokens[2],
                category = tokens[3],
                aboveGroundFloorNumber = tokens[4],
                underGroundFloorNumber = tokens[5],
                storeyHeight = tokens[6],
                dewHeight = tokens[7],
                elevationDifference = tokens[8],
                roofHeight = tokens[9],
                useInArea = tokens[10],
                description = tokens[11],
                version = tokens[12]
            };
            return foundation;
        }

        public int CompareTo(ThAreaFrameFoundation other)
        {
            string pattern = @"\d+";
            Match tm = Regex.Match(this.name, pattern);
            Match om = Regex.Match(other.name, pattern);
            if (tm.Success && om.Success)
            {
                int tv = int.Parse(tm.Value);
                int ov = int.Parse(om.Value);
                return (tv == ov) ? 0 : (Math.Max(tv, ov) == tv ? 1 : -1);
            }
            throw new ArgumentException();
        }

        public double StoreyHeight(int floor)
        {
            string[] tokens = storeyHeight.Split('%');
            if (tokens.Length == 1)
            {
                return double.Parse(tokens[0]);
            }

            for (int i = 1; i < tokens.Length; i++)
            {
                string[] pair = tokens[i].Split('!');
                var floors = ThAreaFrameUtils.ParseStoreyString(pair[0]);
                if (floors.Contains(floor))
                    return double.Parse(pair[1]);
            }

            return double.Parse(tokens[0]);
        }
    }
}
