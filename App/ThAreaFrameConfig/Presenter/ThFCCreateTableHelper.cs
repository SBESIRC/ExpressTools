using System;
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
                            if (engine.Compartments.Count > 1)
                            {
                                table.InsertRowsAndInherit(table.Rows.Count, table.Rows.Count - 1, engine.Compartments.Count - 1);
                                table.MergeCells(CellRange.Create(table, 2, 0, table.Rows.Count - 1, 0));
                            }
                        }
                    }

                    // 填充数据
                    using (AcTransaction tr = new AcTransaction())
                    {
                        Table table = (Table)tr.Transaction.GetObject(id, OpenMode.ForRead);
                        using (new WriteEnabler(table))
                        {
                            // "子项编号"
                            table.Cells[2, 0].Value = (int)engine.Subkey;

                            int column = 1;
                            int dataRow = 2;
                            foreach (ThFireCompartment compartment in engine.Compartments)
                            {
                                if (!compartment.IsDefined)
                                {
                                    continue;
                                }

                                // "防火分区名称"
                                table.Cells[dataRow, column++].Value = compartment.SerialNumber;

                                // "面积（m2）（含敞廊面积）"
                                table.Cells[dataRow, column++].SetAreaValue(compartment.Area, 1.00);

                                // "功能"
                                table.Cells[dataRow, column++].Value = "商业";

                                // "位置"
                                table.Cells[dataRow, column++].SetStoreyValue(compartment.Storey);

                                // "百人疏散宽度（m/百人）"
                                table.Cells[dataRow, column++].SetDensityValue(engine.EvacuationDensity());

                                // "人员密度（人/m2）"
                                table.Cells[dataRow, column++].SetDensityValue(engine.OccupantDensity(compartment));

                                // "应有疏散密度（m）"
                                table.Cells[dataRow, column++].SetDensityValue(
                                    compartment.Area *
                                    engine.EvacuationDensity() *
                                    engine.OccupantDensity(compartment) /
                                    100.00);

                                // "实际疏散密度（m）"
                                table.Cells[dataRow, column++].SetDensityValue(compartment.EvacuationDensity);

                                // "安全出口数量（个）"
                                table.Cells[dataRow, column++].Value = (int)compartment.EmergencyExit;

                                // "是否设置自动灭火系统"
                                table.Cells[dataRow, column++].Value = compartment.SelfExtinguishingSystem ? "是" : "否";

                                // 重置开始列
                                column = 1;

                                // 更新开始行
                                dataRow++;
                            }
                        }
                    }
                }
            }
        }


        public static void CreateFCUndergroundParkingTable(this IThFireCompartmentPresenterCallback presenterCallback,
            ThFCUnderGroundParkingSettings settings)
        {
            using (Active.Document.LockDocument())
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    // 选取插入点
                    PromptPointResult pr = Active.Editor.GetPoint("\n请输入表格插入点: ");
                    if (pr.Status != PromptStatus.OK)
                        return;

                    // 创建表单
                    Point3d position = pr.Value.TransformBy(Active.Editor.CurrentUserCoordinateSystem);
                    ObjectId id = ThFireCompartmentTableBuilder.CreateFCUndergroundParkingAreaTable(position);

                    // 创建引擎
                    ThFCUndergroundParkingEngine engine = new ThFCUndergroundParkingEngine(settings);

                    // 添加数据行
                    using (AcTransaction tr = new AcTransaction())
                    {
                        Table table = (Table)tr.Transaction.GetObject(id, OpenMode.ForRead);
                        using (new WriteEnabler(table))
                        {
                            if (engine.Compartments.Count > 1)
                            {
                                table.InsertRowsAndInherit(table.Rows.Count, table.Rows.Count - 1, engine.Compartments.Count - 1);
                            }
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
                                if (!compartment.IsDefined)
                                {
                                    continue;
                                }

                                // "防火分区名称"
                                table.Cells[dataRow, column++].Value = compartment.SerialNumber;

                                // "防火分区功能"
                                table.Cells[dataRow, column++].Value = "车库";

                                // "防火分区面积"
                                table.Cells[dataRow, column++].SetAreaValue(compartment.Area, 1.00);

                                // "最远疏散距离"
                                table.Cells[dataRow, column++].SetDistanceValue(compartment.EvacuationDistance);

                                // "安全出口个数"
                                table.Cells[dataRow, column++].Value = (int)compartment.EmergencyExit;

                                // 重置开始列
                                column = 0;

                                // 更新开始行
                                dataRow++;
                            }
                        }
                    }
                }
            }
        }
    }
}
