using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using AcHelper;
using AcHelper.Wrappers;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.Presenter
{
    public static class ThFCCreateTableHelper
    {
        public static void CreateFCCommerceTable(this IThFireCompartmentPresenterCallback presenterCallback,
            ThFCCommerceSettings settings)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    // 验证设置参数
                    if (!settings.Compartments.Where(o => o.IsDefined).Any())
                    {
                        return;
                    }

                    // 选取插入点
                    PromptPointResult pr = Active.Editor.GetPoint("\n请输入表格插入点: ");
                    if (pr.Status != PromptStatus.OK)
                        return;

                    // 创建表单
                    Point3d position = pr.Value.TransformBy(Active.Editor.CurrentUserCoordinateSystem);
                    ObjectId id = ThFireCompartmentTableBuilder.CreateFCCommerceAreaTable(position);

                    // 创建引擎
                    ThFCCommerceEngine engine = new ThFCCommerceEngine(settings);

                    // 添加数据行
                    using (AcTransaction tr = new AcTransaction())
                    {
                        Table table = (Table)tr.Transaction.GetObject(id, OpenMode.ForRead);
                        using (new WriteEnabler(table))
                        {
                            table.InsertRowsAndInherit(table.Rows.Count, table.Rows.Count - 1, engine.Compartments.Count);
                        }
                    }

                    // 填充数据
                    using (AcTransaction tr = new AcTransaction())
                    {
                        Table table = (Table)tr.Transaction.GetObject(id, OpenMode.ForRead);
                        using (new WriteEnabler(table))
                        {
                            int column = 0;
                            int dataRow = 2;
                            foreach (ThFireCompartment compartment in engine.Compartments)
                            {
                                // "子项编号"
                                table.Cells[dataRow, column++].Value = engine.Subkey;

                                // "耐火等级"
                                column++;

                                // "防火分区名称"
                                table.Cells[dataRow, column++].Value = compartment.SerialNumber;

                                // "面积（m2）（含敞廊面积）"
                                table.Cells[dataRow, column++].SetAreaValue(compartment.Area, 1.00);

                                // "功能"
                                table.Cells[dataRow, column++].Value = "商业";

                                // "位置"
                                table.Cells[dataRow, column++].SetStoreyValue(compartment.Storey);

                                // "地下楼层与地面出入口地面高差是否≤10m"
                                column++;

                                // "百人疏散宽度（m/百人）"
                                table.Cells[dataRow, column++].SetDensityValue(engine.EvacuationDensity(compartment));

                                // "人员密度（人/m2）"
                                table.Cells[dataRow, column++].SetDensityValue(engine.OccupantDensity(compartment));

                                // "应有疏散密度（m）"
                                table.Cells[dataRow, column++].SetDensityValue(
                                    compartment.Area *
                                    engine.EvacuationDensity(compartment) *
                                    engine.OccupantDensity(compartment) /
                                    100.00);

                                // "实际疏散密度（m）"
                                table.Cells[dataRow, column++].SetDensityValue(0.00);

                                // "安全出口数量（个）"
                                table.Cells[dataRow, column++].Value = 0;

                                // "是否设置自动灭火系统"
                                table.Cells[dataRow, column++].Value = compartment.SelfExtinguishingSystem ? "是" : "否";
                            }
                        }
                    }
                }
            }
        }
    }
}
