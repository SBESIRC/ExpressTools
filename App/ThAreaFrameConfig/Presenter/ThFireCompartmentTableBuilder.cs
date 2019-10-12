using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using DotNetARX;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrameConfig.Presenter
{
    public static class ThAreaFrameTable
    {
        private const string AreaDataFormat = "%lu2%pr2";
        private const string DensityDataFormat = "%lu2%pr2";
        private const string DistanceDataFormat = "%lu2%pr2";
        private const string PercentageDataFormat = "%lu2%pr2%ps[,%]%ct8[100]";
        private const double AreaUnitScale = (1.0 / 1000000.0);

        public static void SetAreaValue(this Cell c, double area, double unitScale = AreaUnitScale)
        {
            c.Value = area * unitScale;
            c.DataFormat = AreaDataFormat;
            c.DataType = new DataTypeParameter(DataType.Double, UnitType.Area);
        }

        public static void SetDensityValue(this Cell c, double density)
        {
            c.Value = density;
            c.DataFormat = DensityDataFormat;
            c.DataType = new DataTypeParameter(DataType.Double, UnitType.Unitless);
        }

        public static void SetDistanceValue(this Cell c, double distance)
        {
            c.Value = distance;
            c.DataFormat = DistanceDataFormat;
            c.DataType = new DataTypeParameter(DataType.Double, UnitType.Distance);
        }

        public static void SetStoreyValue(this Cell c, int storey)
        {
            c.Value = string.Format("地上{0}层", storey.NumberToChinese());
        }

        public static void SetPercentageValue(this Cell c, double value)
        {
            c.Value = value;
            c.DataFormat = PercentageDataFormat;
        }
    }

    public class ThFireCompartmentTableBuilder
    {
        public static double TextHeight = 3000;
        public static double ColumnWidth = 25000;

        public static ObjectId CreateFCCommerceAreaTable(Point3d position)
        {
            Table table = new Table()
            {
                Position = position,
                TableStyle = Active.Database.Tablestyle
            };
            table.SetSize(3, 11);
            table.SetRowHeight(5000);
            table.SetTextHeight(3000);
            table.SetColumnWidth(25000);
            table.SetAlignment(CellAlignment.MiddleCenter);
            table.GenerateLayout();

            // 第一行：标题
            table.Cells[0, 0].TextString = "商业防火分区疏散宽度表";

            // 第二行：表头
            string[] headers =
            {
                "子项编号",
                "防火分区名称",
                "面积（m\u00B2）\n（含敞廊面积）",
                "功能",
                "位置",
                "百人疏散宽度\n（m/百人）",
                "人员密度\n（人/m\u00B2）",
                "应有疏散密度\n（m）",
                "实际疏散密度\n（m）",
                "安全出口数量\n（个）",
                "是否设置自动灭火系统"
            };
            table.Rows[1].Height = 20000;
            table.SetRowTextString(1, headers);

            // "子项编号"
            table.Cells[2, 0].DataType = new DataTypeParameter(DataType.Long, UnitType.Unitless);

            // "防火分区名称"
            table.Cells[2, 1].DataType = new DataTypeParameter(DataType.String, UnitType.Unitless);

            // "面积"
            table.Cells[2, 2].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);

            // "功能"
            table.Cells[2, 3].DataType = new DataTypeParameter(DataType.String, UnitType.Unitless);

            // "位置"
            table.Cells[2, 4].DataType = new DataTypeParameter(DataType.String, UnitType.Unitless);

            // "百人疏散宽度"
            table.Cells[2, 5].DataType = new DataTypeParameter(DataType.Double, UnitType.Unitless);

            // "人员密度"
            table.Cells[2, 6].DataType = new DataTypeParameter(DataType.Double, UnitType.Unitless);

            // "应有疏散密度"
            table.Cells[2, 7].DataType = new DataTypeParameter(DataType.Double, UnitType.Unitless);

            // "实际疏散密度"
            table.Cells[2, 8].DataType = new DataTypeParameter(DataType.Double, UnitType.Unitless);

            // "安全出口数量"
            table.Cells[2, 9].DataType = new DataTypeParameter(DataType.Long, UnitType.Unitless);

            // "是否设置自动灭火系统"
            table.Cells[2, 10].DataType = new DataTypeParameter(DataType.String, UnitType.Unitless);

            // 创建表单
            return Active.Database.AddToModelSpace(table);
        }


        public static ObjectId CreateFCUndergroundParkingAreaTable(Point3d position)
        {
            Table table = new Table()
            {
                Position = position,
                TableStyle = Active.Database.Tablestyle
            };
            table.SetSize(3, 5);
            table.SetRowHeight(5000);
            table.SetTextHeight(3000);
            table.SetColumnWidth(25000);
            table.SetAlignment(CellAlignment.MiddleCenter);
            table.GenerateLayout();

            // 第一行：标题
            table.Cells[0, 0].TextString = "地下车库防火分区疏散宽度表";

            // 第二行：表头
            string[] headers =
            {
                "防火分区名称",
                "防火分区功能",
                "防火分区面积\n（m\u00B2）",
                "最远疏散距离\n（m）",
                "安全出口个数"
            };
            table.Rows[1].Height = 20000;
            table.SetRowTextString(1, headers);

            // "防火分区名称"
            table.Cells[2, 0].DataType = new DataTypeParameter(DataType.String, UnitType.Unitless);

            // "防火分区功能"
            table.Cells[2, 1].DataType = new DataTypeParameter(DataType.String, UnitType.Unitless);

            // "防火分区面积"
            table.Cells[2, 2].DataType = new DataTypeParameter(DataType.Double, UnitType.Area);

            // "最远疏散距离"
            table.Cells[2, 3].DataType = new DataTypeParameter(DataType.Double, UnitType.Distance);

            // "安全出口数量"
            table.Cells[2, 4].DataType = new DataTypeParameter(DataType.Long, UnitType.Unitless);

            // 创建表单
            return Active.Database.AddToModelSpace(table);
        }
    }
}
