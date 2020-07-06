using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    /// <summary>
    /// 需要绘制的柱子信息
    /// </summary>
    public class DrawColumnInf
    {
        /// <summary>
        /// 柱号
        /// </summary>
        public int JtID { get; set; }
        /// <summary>
        /// 自然层号
        /// </summary>
        public int FloorID { get; set; }
        /// <summary>
        /// 标准层号
        /// </summary>
        public int StdFlrID { get; set; }
        /// <summary>
        /// 中心X偏移
        /// </summary>
        public double EccX { get; set; }
        /// <summary>
        /// 中心Y偏移
        /// </summary>
        public double EccY { get; set; }
        /// <summary>
        /// 柱旋转
        /// </summary>
        public double Rotation { get; set; }
        public string Spec
        {
            get
            {
                if (!string.IsNullOrEmpty(this.ShapeVal))
                {
                    string[] strs = this.ShapeVal.Split(',');
                    if (strs.Length > 3)
                    {
                        return strs[1] + "x" + strs[2];
                    }
                }
                return "";
            }
        }
        /// <summary>
        /// 柱子规格
        /// </summary>
        public string ShapeVal { get; set; } = "";
        public double X { get; set; }
        public double Y { get; set; }
    }
}
