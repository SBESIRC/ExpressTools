using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using AcHelper.Wrappers;
using System;
using System.Windows.Forms;

[assembly: CommandClass(typeof(ThAreaFrame.ThAreaFrameCommands))]
[assembly: ExtensionApplication(typeof(ThAreaFrame.ThAreaFrameApp))]

namespace ThAreaFrame
{
    public class ThAreaFrameApp : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }
    }

    public class ThAreaFrameCommands
    {
        [CommandMethod("TIANHUACAD", "THBAC", CommandFlags.Modal)]
        public void AreaCommand()
        {
            var ed = Active.Editor;

            // 选取插入点
            PromptPointResult pr = ed.GetPoint("\n请输入表格插入点: ");
            if (pr.Status != PromptStatus.OK)
                return;

            // 创建面积引擎
            ThAreaFrameDriver driver = ThAreaFrameDriver.ResidentialDriver();
            if (driver.engines.Count == 0)
            {
                ed.WriteMessage("\n未找到指定目录，请将单体图纸放置在名为\"建筑单体\"的子目录下。");
                return;
            }

            // 创建表单
            Point3d position = pr.Value.TransformBy(ed.CurrentUserCoordinateSystem);
            ObjectId id = ThAreaFrameTableBuilder.CreateBuildingAreaTable(position);
            using (AcTransaction tr = new AcTransaction())
            {
                Table table = (Table)tr.Transaction.GetObject(id, OpenMode.ForRead);
                using (new WriteEnabler(table))
                {
                    int column = 0;
                    int headerRow = (table.Rows.Count - 2);

                    // 建筑编号
                    column++;

                    // 普通楼层面积
                    int ordinaryStoreyCount = driver.OrdinaryStoreyCollection.Count;
                    if (ordinaryStoreyCount > 0)
                    {
                        table.InsertColumns(column, ThAreaFrameTableBuilder.ColumnWidth, ordinaryStoreyCount);
                        foreach (var storey in driver.OrdinaryStoreyCollection)
                        {
                            table.Cells[headerRow, column].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[headerRow, column].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[headerRow, column].Value = ThAreaFrameTableBuilder.OrdinaryStoreyColumnHeader(storey);
                            column++;
                        }
                    }

                    // 标准楼层
                    int standardStoreyCount = Math.Max(driver.StandardStoreyCount(), driver.StandardAOccupancyStoreyCount());
                    if (standardStoreyCount > 0)
                    {
                        table.InsertColumns(column, ThAreaFrameTableBuilder.ColumnWidth, standardStoreyCount);
                        for (int index = 0; index < standardStoreyCount; index++)
                        {
                            table.Cells[headerRow, column + index].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[headerRow, column + index].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[headerRow, column + index].Value = ThAreaFrameTableBuilder.StandardStoreyColumnHeader(index);
                        }

                        column += standardStoreyCount;
                    }

                    // 地上层数
                    column++;

                    // 出屋面楼梯间及屋顶机房计容面积
                    column++;

                    // 计容面积
                    column++;

                    // 地下楼层
                    int underGroundStoreyCount = driver.UnderGroundStoreyCollection.Count;
                    if (underGroundStoreyCount > 0)
                    {
                        table.InsertColumns(column, ThAreaFrameTableBuilder.ColumnWidth, underGroundStoreyCount);
                        foreach (var storey in driver.UnderGroundStoreyCollection)
                        {
                            table.Cells[headerRow, column].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[headerRow, column].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[headerRow, column].Value = ThAreaFrameTableBuilder.UnderGroundStoreyColumnHeader(storey);
                            column++;
                        }
                    }

                    // 地下建筑面积
                    column++;

                    // 楼栋基底面积
                    column++;

                    // 架空层建筑面积
                    column++;

                    // "总建筑面积（不含架空层）"
                    column++;
                }
            }

            // 添加数据行
            using (AcTransaction tr = new AcTransaction())
            {
                Table table = (Table)tr.Transaction.GetObject(id, OpenMode.ForRead);
                using (new WriteEnabler(table))
                {
                    table.InsertRowsAndInherit(table.Rows.Count, table.Rows.Count - 1, driver.engines.Count);
                }
            }

            // 填充数据
            using (AcTransaction tr = new AcTransaction())
            {
                Table table = (Table)tr.Transaction.GetObject(id, OpenMode.ForRead);
                using (new WriteEnabler(table))
                {
                    // 配置进度条
                    ProgressMeter pm = new ProgressMeter();
                    pm.Start(@"正在处理图纸");
                    pm.SetLimit(driver.engines.Count);

                    int column = 0;
                    int dataRow = 2;
                    foreach (ThAreaFrameEngine engine in driver.engines)
                    {
                        // 更新进度条
                        pm.MeterProgress();

                        // 建筑编号
                        table.Cells[dataRow, column++].Value = engine.Name;

                        // 普通楼层面积
                        foreach(int storey in engine.OrdinaryStoreyCollection)
                        {
                            int index = driver.OrdinaryStoreyCollection.FindIndex(x => x == storey);
                            table.Cells[dataRow, column + index].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[dataRow, column + index].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[dataRow, column + index].SetAreaValue(engine.AreaOfFloor(storey, true));
                        }
                        column += driver.OrdinaryStoreyCollection.Count;

                        // 标准楼层
                        //  区分两种类型的标准楼层：住宅构件，附属公建
                        //  顺序：住宅构件在前，附属公建在后
                        int standardStoreyCount = 0;
                        for (int i = 0; i < engine.Building.StandardStoreys().Count; i++)
                        {
                            table.Cells[dataRow, column + standardStoreyCount + i].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[dataRow, column + standardStoreyCount + i].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[dataRow, column + standardStoreyCount + i].SetAreaValue(engine.Calculators["住宅构件"].AreaOfStandardStoreys(true)[i]);

                        }
                        standardStoreyCount += engine.Building.StandardStoreys().Count;
                        for (int i = 0; i < engine.AOccupancyBuilding.StandardStoreys().Count; i++)
                        {
                            table.Cells[dataRow, column + standardStoreyCount + i].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[dataRow, column + standardStoreyCount + i].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[dataRow, column + standardStoreyCount + i].SetAreaValue(engine.Calculators["附属公建"].AreaOfStandardStoreys(true)[i]);
                        }
                        column += driver.StandardStoreyCount();

                        // 地上层数
                        table.Cells[dataRow, column++].Value = engine.AboveGroundStoreyNumber;

                        // 出屋面楼梯间及屋顶机房计容面积
                        table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfRoof(true));

                        // 计容面积
                        table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfCapacityBuilding(engine.AreaOfRoof(true)));

                        // 地下楼层
                        foreach(int storey in engine.UnderGroundStoreyCollection)
                        {
                            int index = driver.UnderGroundStoreyCollection.FindIndex(x => x == storey);
                            table.Cells[dataRow, column + index].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[dataRow, column + index].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[dataRow, column + index].SetAreaValue(engine.AreaOfFloor(storey));
                        }
                        column += driver.UnderGroundStoreyCollection.Count;

                        // 地下建筑面积
                        table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfUnderGround());

                        // 楼栋基底面积
                        table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfFoundation());

                        // 架空层建筑面积
                        table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfStilt());

                        // "总建筑面积（不含架空层）"
                        table.Cells[dataRow, column++].SetAreaValue(engine.AreaOfAboveGround(engine.AreaOfRoof(false)) 
                            + engine.AreaOfUnderGround());

                        // 重置开始列
                        column = 0;
                        // 更新开始行
                        dataRow++;

                        // 处理面积引擎
                        engine.Dispose();

                        // 让CAD在长时间任务处理时任然能接收消息
                        Application.DoEvents();
                    }

                    // 统计总计
                    dataRow = table.Rows.Count - 1;

                    // 建筑编号
                    table.Cells[dataRow, 0].Value = "总计";

                    // "楼栋计容面积"
                    string[] headers =
                    {
                        "楼栋计容面积",
                        "地下建筑面积",
                        "楼栋基底面积",
                        "架空层建筑面积",
                        "总建筑面积（不含架空层）"
                    };
                    foreach(string header in headers)
                    {
                        column = ThAreaFrameTableBuilder.ColumnIndex(table, header);
                        if (column > 0)
                        {
                            double area = 0.0;
                            for (int i = 2; i < dataRow; i++)
                            {
                                area += (double)table.Cells[i, column].Value;
                            }
                            table.Cells[dataRow, column].Alignment = CellAlignment.MiddleCenter;
                            table.Cells[dataRow, column].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                            table.Cells[dataRow, column].SetAreaValue(area, 1.0);
                        }
                    }

                    pm.Stop();  // 停止进度条更新
                }
            }
        }


        [CommandMethod("TIANHUACAD", "THTET", CommandFlags.Modal)]
        public void AreaCommand2()
        {
            var ed = Active.Editor;

            // 选取插入点
            PromptPointResult pr = ed.GetPoint("\n请输入表格插入点: ");
            if (pr.Status != PromptStatus.OK)
                return;

            // 创建单体面积引擎
            ThAreaFrameDriver driver = ThAreaFrameDriver.ResidentialDriver();
            if (driver.engines.Count == 0)
            {
                ed.WriteMessage("\n未找到指定目录，请将单体图纸放置在名为\"建筑单体\"的子目录下。");
                return;
            }

            // 创建总和面积引擎
            ThAreaFrameMasterEngine engine = ThAreaFrameMasterEngine.Engine();

            // 创建表单
            Point3d position = pr.Value.TransformBy(ed.CurrentUserCoordinateSystem);
            ObjectId id = ThAreaFrameTableBuilder.CreateMainIndexesTable(position);

            // 填充数据
            using (AcTransaction tr = new AcTransaction())
            {
                Table table = (Table)tr.Transaction.GetObject(id, OpenMode.ForRead);
                using (new WriteEnabler(table))
                {
                    // "规划净用地面积"
                    table.Cells[2, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[2, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[2, 3].SetAreaValue(engine.AreaOfPlanning());

                    // "住宅建筑面积"
                    table.Cells[6, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[6, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[6, 3].SetAreaValue(driver.ResidentAreaOfAboveGround());

                    // "商业建筑面积"
                    table.Cells[7, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[7, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[7, 3].SetAreaValue(driver.AOccupancyAreaOfAboveGround());

                    // "其他建筑面积"
                    table.Cells[8, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[8, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[8, 3].SetAreaValue(0);

                    // "架空部分建筑面积（非计容）"
                    table.Cells[9, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[9, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[9, 3].SetAreaValue(driver.AreaOfStilt());

                    // "地下室主楼建筑面积"
                    table.Cells[11, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[11, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[11, 3].SetAreaValue(driver.AreaOfUnderGround());

                    // "地下室其他建筑面积"
                    table.Cells[12, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[12, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[12, 3].SetAreaValue(driver.AreaOfParkingGarage());

                    // "建筑基底面积"
                    table.Cells[14, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[14, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[14, 3].SetAreaValue(driver.AreaOfFoundation());

                    // "地上停车位（个）"
                    table.Cells[16, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[16, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[16, 3].Value = engine.CountOfAboveGroundParkingLot();

                    // "地下停车位（个）"
                    table.Cells[17, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[17, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[17, 3].Value = driver.CountOfUnderGroundParkingSlot();

                    // "地上非机动停车位（个）"
                    table.Cells[19, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[19, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[19, 3].Value = 0;

                    // "地下非机动停车位（个）"
                    table.Cells[20, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[20, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[20, 3].Value = 0;

                    // "建筑密度"
                    table.Cells[21, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[21, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[21, 3].SetPercentageValue((double)table.Cells[14, 3].Value / (double)table.Cells[2, 3].Value);

                    // "绿地率"
                    table.Cells[22, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[22, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    double greenSpace = engine.AreaOfGreenSpace() + driver.AreaOfRoofGreenSpace();
                    table.Cells[22, 3].SetPercentageValue(greenSpace/ engine.AreaOfPlanning());

                    // "居住户数"
                    table.Cells[23, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[23, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[23, 3].Value = engine.CountOfHousehold();

                    // "居住人数"
                    table.Cells[24, 3].Alignment = CellAlignment.MiddleCenter;
                    table.Cells[24, 3].TextHeight = ThAreaFrameTableBuilder.TextHeight;
                    table.Cells[24, 3].Value = engine.CountOfHouseholdPopulation();
                }
            }
        }
    }
}
