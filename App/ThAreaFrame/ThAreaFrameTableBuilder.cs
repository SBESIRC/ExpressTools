using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using DotNetARX;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrame
{
    public static class ThAreaFrameTable
    {
        private const string AreaDataFormat = "%lu2%pr3";
        private const double AreaUnitScale = (1.0 / 1000000.0);

        public static void SetAreaValue(this Cell c, double area)
        {
            c.Value = area * AreaUnitScale;
            c.DataFormat = AreaDataFormat;
            c.DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
        }
    }


    public class ThAreaFrameTableBuilder
    {
        public static double TextHeight = 3000;
        public static double ColumnWidth = 25000;

        public static string OrdinaryStoreyColumnHeader(int storey)
        {
            return storey.NumberToChinese() + "层";
        }

        public static string UnderGroundStoreyColumnHeader(int storey)
        {
            return "地下" + (-storey).NumberToChinese() + "层";
        }

        public static string StandardStoreyColumnHeader(int index)
        { 
            return "标准层" + (char)(65 + index);
        }

        public static ObjectId CreateBuildingAreaTable(Point3d position)
        {
            Table table = new Table()
            {
                Position = position,
                TableStyle = Active.Database.Tablestyle
            };
            table.SetSize(3, 7);
            table.SetRowHeight(5000);
            table.SetTextHeight(3000);
            table.SetColumnWidth(25000);
            table.SetAlignment(CellAlignment.MiddleCenter);
            table.GenerateLayout();

            // 第一行：标题
            table.Cells[0, 0].TextString = "单体面积计算表";

            // 第二行：表头
            string[] headers =
            {
                "楼号",
                "地上层数",
                "出屋面楼梯间及屋顶机房",
                "报建面积",
                "计容面积",
                "地下总面积",
                "楼栋基底面积"
            };
            table.Rows[1].Height = 20000;
            table.SetRowTextString(1, headers);

            // "楼号"
            table.Cells[2, 0].DataType = new DataTypeParameter(DataType.String, UnitType.Unitless);

            // "地上层数"
            table.Cells[2, 1].DataType = new DataTypeParameter(DataType.Long,   UnitType.Unitless);

            // "出屋面楼梯间及屋顶机房"
            table.Cells[2, 2].DataFormat = "%lu2%pr3";
            table.Cells[2, 2].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);

            // "报建面积"
            table.Cells[2, 3].DataFormat = "%lu2%pr3";
            table.Cells[2, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);

            // "计容面积"
            table.Cells[2, 4].DataFormat = "%lu2%pr3";
            table.Cells[2, 4].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);

            // "地下总面积"
            table.Cells[2, 5].DataFormat = "%lu2%pr3";
            table.Cells[2, 5].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);

            // "楼栋基底面积"
            table.Cells[2, 6].DataFormat = "%lu2%pr3";
            table.Cells[2, 6].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);

            // 创建表单
            return Active.Database.AddToModelSpace(table);
        }
    }
}
