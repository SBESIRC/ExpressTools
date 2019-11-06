using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace ThColumnInfo.Model
{
    public class TableCellInfo
    {
        /// <summary>
        /// 行
        /// </summary>
        public int Row { get; set; }
        /// <summary>
        /// 列
        /// </summary>
        public int Column { get; set; }
        /// <summary>
        /// 单元格中内容
        /// </summary>
        public string Text { get; set; } = "";
        public Point3dCollection BoundaryPts { get; set; } = new Point3dCollection();
        public double RowHeight { get; set; }
        public double ColumnWidth { get; set; }
        /// <summary>
        /// 行跨距
        /// </summary>
        public int RowSpan { get; set; }
        /// <summary>
        /// 列跨距
        /// </summary>
        public int ColumnSpan { get; set; }
    }
}
