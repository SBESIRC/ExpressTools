using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace ThColumnInfo
{
    public class ColumnRelateInf
    {
        /// <summary>
        /// DataBase中的柱子信息
        /// </summary>
        public DrawColumnInf DbColumnInf
        {
            get;
            set;
        }
        /// <summary>
        /// DataBase中的柱子在Cad图纸的位置
        /// </summary>
        public List<Point3d> InModelPts
        {
            get;
            set;
        }
        /// <summary>
        /// 关联到的模型中实际的柱子
        /// </summary>
        public List<ColumnInf> ModelColumnInfs
        {
            get;
            set;
        }
        /// <summary>
        /// 自定义数据
        /// </summary>
        public ColumnCustomData CustomData
        {
            get;
            set;
        }
        /// <summary>
        /// 埋入数据后存储的数据
        /// </summary>

        public YjkColumnDataInfo YjkColumnData
        {
            get;
            set;
        }
    }
}
