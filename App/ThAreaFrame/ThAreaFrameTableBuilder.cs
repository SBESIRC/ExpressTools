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

        public static void SetAreaValue(this Cell c, double area, double unitScale = AreaUnitScale)
        {
            c.Value = area * unitScale;
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

        public static int ColumnIndex(Table table, string header)
        {
            const int HEADER_ROW = 1;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Cells[HEADER_ROW, i].TextString == header)
                    return i;
            }
            return -1;
        }

        public static ObjectId CreateBuildingAreaTable(Point3d position)
        {
            Table table = new Table()
            {
                Position = position,
                TableStyle = Active.Database.Tablestyle
            };
            table.SetSize(3, 8);
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
                "出屋面楼梯间及屋顶机房计容面积计容面积",
                "楼栋计容面积",
                "地下建筑面积",
                "楼栋基底面积",
                "架空层建筑面积",
                "总建筑面积（不含架空层）"
            };
            table.Rows[1].Height = 20000;
            table.SetRowTextString(1, headers);

            // "楼号"
            table.Cells[2, 0].DataType = new DataTypeParameter(DataType.String, UnitType.Unitless);

            // "地上层数"
            table.Cells[2, 1].DataType = new DataTypeParameter(DataType.Long,   UnitType.Unitless);

            // "出屋面楼梯间及屋顶机房计容面积"
            table.Cells[2, 2].DataFormat = "%lu2%pr3";
            table.Cells[2, 2].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);

            // "计容面积"
            table.Cells[2, 3].DataFormat = "%lu2%pr3";
            table.Cells[2, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);

            // "地下建筑面积"
            table.Cells[2, 4].DataFormat = "%lu2%pr3";
            table.Cells[2, 4].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);

            // "楼栋基底面积"
            table.Cells[2, 5].DataFormat = "%lu2%pr3";
            table.Cells[2, 5].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);

            // "架空层建筑面积"
            table.Cells[2, 6].DataFormat = "%lu2%pr3";
            table.Cells[2, 6].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);

            // "总建筑面积（不含架空层）"
            table.Cells[2, 7].DataFormat = "%lu2%pr3";
            table.Cells[2, 7].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);

            // 创建表单
            return Active.Database.AddToModelSpace(table);
        }


        public static ObjectId CreateMainIndexesTable(Point3d position)
        {
            Table table = new Table()
            {
                Position = position,
                TableStyle = Active.Database.Tablestyle
            };
            table.SetSize(25, 4);
            table.SetRowHeight(10763.01);
            table.SetTextHeight(5000);
            table.Columns[0].Width = 7379.62;
            table.Columns[1].Width = 8332.07;
            table.Columns[2].Width = 79288.31;
            table.Columns[3].Width = 95000;
            table.SetAlignment(CellAlignment.MiddleCenter);
            table.GenerateLayout();

            // 第一行：标题
            table.Cells[0, 0].TextString = "综合经济技术指标表";

            // 第二行：表头
            table.Rows[1].Height = 20000;
            table.MergeCells(CellRange.Create(table, 1, 0, 1, 2));
            table.Cells[1, 0].TextString = "项目";
            table.Cells[1, 3].TextString = "指标";

            int dataRow = 2;

            // "规划净用地面积"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow, 2));
            table.Cells[dataRow, 0].Value = "规划净用地面积";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            dataRow++;

            // "总建筑面积"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow, 2));
            table.Cells[dataRow, 0].Value = "总建筑面积";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            table.Cells[dataRow, 3].TextString = "=D5+D11"; //公式：（总建筑面积 = 地上总建筑面积 + 地下建筑面积）
            dataRow++;

            // "地上总建筑面积"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow, 2));
            table.Cells[dataRow, 0].Value = "地上总建筑面积";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            table.Cells[dataRow, 3].TextString = "=D6+D10"; //公式：（地上总建筑面积 = 地上计容建筑面积 + 架空部分建筑面积（非计容））
            dataRow++;

            // "其中"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow + 4, 0));
            table.Cells[dataRow, 0].Value = "其中";

            // "其中"的"其中"
            table.MergeCells(CellRange.Create(table, dataRow + 1, 1, dataRow + 3, 1));
            table.Cells[dataRow + 1, 1].Value = "其中";

            // "地上计容建筑面积"
            table.MergeCells(CellRange.Create(table, dataRow, 1, dataRow, 2));
            table.Cells[dataRow, 1].Value = "地上计容建筑面积";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            table.Cells[dataRow, 3].TextString = "=D7+D8+D9";   //公式：(地上计容建筑面积 = 住宅建筑面积 + 商业建筑面积 + 其他建筑面积)
            dataRow++;

            // "住宅建筑面积"
            table.Cells[dataRow, 2].Value = "住宅建筑面积";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            dataRow++;

            // "商业建筑面积"
            table.Cells[dataRow, 2].Value = "商业建筑面积";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            dataRow++;

            // "其他建筑面积"
            table.Cells[dataRow, 2].Value = "其他建筑面积";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            dataRow++;

            // "架空部分建筑面积（非计容）"
            table.MergeCells(CellRange.Create(table, dataRow, 1, dataRow, 2));
            table.Cells[dataRow, 1].Value = "架空部分建筑面积（非计容）";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            dataRow++;

            // "地下建筑面积"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow, 2));
            table.Cells[dataRow, 0].Value = "地下建筑面积";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            table.Cells[dataRow, 3].TextString = "=D12+D13";    //公式：（地下建筑面积 = 地下室主楼建筑面积 + 地下室其他建筑面积）
            dataRow++;

            // "其中"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow + 1, 0));
            table.Cells[dataRow, 0].Value = "其中";

            // "地下室主楼建筑面积"
            table.MergeCells(CellRange.Create(table, dataRow, 1, dataRow, 2));
            table.Cells[dataRow, 1].Value = "地下室主楼建筑面积";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            dataRow++;

            // "地下室其他建筑面积"
            table.MergeCells(CellRange.Create(table, dataRow, 1, dataRow, 2));
            table.Cells[dataRow, 1].Value = "地下室其他建筑面积";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            dataRow++;

            // "容积率"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow, 2));
            table.Cells[dataRow, 0].Value = "容积率";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            table.Cells[dataRow, 3].TextString = "=D6/D3";     //公式：（容积率 = 地上计容建筑面积/规划净用地面积）
            dataRow++;

            // "建筑基底面积"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow, 2));
            table.Cells[dataRow, 0].Value = "建筑基底面积";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            dataRow++;

            // "机动车停车位（个）"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow, 2));
            table.Cells[dataRow, 0].Value = "机动车停车位（个）";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Long, UnitType.Unitless);
            table.Cells[dataRow, 3].TextString = "=D17+D18";    //公式：（机动车停车位 = 地上停车位 +地下停车位）
            dataRow++;

            // "其中"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow + 1, 0));
            table.Cells[dataRow, 0].Value = "其中";

            // "地上停车位（个）"
            table.MergeCells(CellRange.Create(table, dataRow, 1, dataRow, 2));
            table.Cells[dataRow, 1].Value = "地上停车位（个）";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Long, UnitType.Unitless);
            dataRow++;

            // "地下停车位（个）"
            table.MergeCells(CellRange.Create(table, dataRow, 1, dataRow, 2));
            table.Cells[dataRow, 1].Value = "地下停车位（个）";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Long, UnitType.Unitless);
            dataRow++;

            // "非机动车停车位（个）"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow, 2));
            table.Cells[dataRow, 0].Value = "非机动车停车位（个）";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Long, UnitType.Unitless);
            table.Cells[dataRow, 3].TextString = "=D20+D21";    //公式：（非机动车停车位 = 地上非机动停车位 + 地下非机动停车位）
            dataRow++;

            // "其中"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow + 1, 0));
            table.Cells[dataRow, 0].Value = "其中";

            // "地上非机动停车位（个）"
            table.MergeCells(CellRange.Create(table, dataRow, 1, dataRow, 2));
            table.Cells[dataRow, 1].Value = "地上非机动停车位（个）";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Long, UnitType.Unitless);
            dataRow++;

            // "地下非机动停车位（个）"
            table.MergeCells(CellRange.Create(table, dataRow, 1, dataRow, 2));
            table.Cells[dataRow, 1].Value = "地下非机动停车位（个）";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Long, UnitType.Unitless);
            dataRow++;

            // "建筑密度"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow, 2));
            table.Cells[dataRow, 0].Value = "建筑密度";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            table.Cells[dataRow, 3].TextString = "=D15/D3"; //公式：（建筑密度 = 建筑基底面积/规划净用地面积）
            dataRow++;

            // "绿地率"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow, 2));
            table.Cells[dataRow, 0].Value = "绿地率";
            table.Cells[dataRow, 3].DataFormat = "%lu2%pr3";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
            dataRow++;

            // "居住户数"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow, 2));
            table.Cells[dataRow, 0].Value = "居住户数";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Long, UnitType.Unitless);
            dataRow++;

            // "居住人数"
            table.MergeCells(CellRange.Create(table, dataRow, 0, dataRow, 2));
            table.Cells[dataRow, 0].Value = "居住人数";
            table.Cells[dataRow, 3].DataType = new DataTypeParameter(DataType.Long, UnitType.Unitless);
            dataRow++;

            // 创建表单
            return Active.Database.AddToModelSpace(table);
        }
    }
}
