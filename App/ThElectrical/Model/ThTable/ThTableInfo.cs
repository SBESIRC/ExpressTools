using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThElectrical.Model.ThTable
{
    public class ThTableInfo
    {
        public ObjectId LineId { get; set; }//表格直线信息
        public Point3d MinPoint { get; set; }
        public Point3d MaxPoint { get; set; }

        public const string LineLayerName = "E-POWR-BUSH";//表格直线图层名
        public const double LineLengthTol = 30000;//表格直线最小长度定义
        public const double LineDeltaYTol = 1;//表格直线的纵向容差

        public ThTableInfo(ObjectId lineId, Point3d minPoint, Point3d maxPoint)
        {
            LineId = lineId;
            MinPoint = minPoint;
            MaxPoint = maxPoint;
        }
    }
}
