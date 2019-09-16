using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Geometry;

namespace THColumnInfo
{
    public class ColumnInf
    {
        /// <summary>
        /// 柱子编号
        /// </summary>
        public string Code { get; set; } = "";
        /// <summary>
        /// 柱子规格
        /// </summary>
        public string Spec { get; set; } = "";
        /// <summary>
        /// 如果柱子边没有柱原位标注，则所有钢筋规格一样
        /// </summary>
        public string IronSpec { get; set; } = "";
        /// <summary>
        /// 四角钢筋规格
        /// </summary>
        public string CornerIronSpec { get; set; } = "";
        /// <summary>
        /// X方向钢筋规格
        /// </summary>
        public string XIronSpec { get; set; } = "";
        /// <summary>
        /// Y方向钢筋规格
        /// </summary>
        public string YIronSpec { get; set; } = "";
        /// <summary>
        /// X方向钢筋数量
        /// </summary>
        public int XIronNum { get; set; } = 0;
        /// <summary>
        /// Y方向钢筋数量
        /// </summary>
        public int YIronNum { get; set; } = 0;
        /// <summary>
        /// 相邻两个箍筋在垂直方向高度
        /// </summary>
        public string NeiborGuJinHeightSpec { get; set; } = "";
        /// <summary>
        /// 柱子对角点（左下角和右上角）
        /// </summary>
        public List<Point3d> Points { get; set; } = new List<Point3d>();
        /// <summary>
        /// 除了编号，其它都没赋值
        /// </summary>
        /// <returns></returns>

        public bool NotSetValueExceptCode()
        {
            bool res = false;
            if (string.IsNullOrEmpty(this.Code))
            {
                return true;
            }
            if (this.Spec == "" && this.IronSpec == "" && this.CornerIronSpec == "" && this.XIronSpec == ""
                && this.YIronSpec == "" && this.NeiborGuJinHeightSpec == "")
            {
                return true;
            }
            return res;
        }
    }
}
