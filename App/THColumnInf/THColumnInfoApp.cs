using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using System.Collections.Generic;
using System.Text;
using ThColumnInfo.Validate;
using ThColumnInfo.ViewModel;
using ThColumnInfo.View;

[assembly: CommandClass(typeof(ThColumnInfo.ThColumnInfoCommands))]
[assembly: ExtensionApplication(typeof(ThColumnInfo.ThColumnInfoApp))]
namespace ThColumnInfo
{
    public class ThColumnInfoApp : IExtensionApplication
    {
        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Terminate()
        {
            //throw new NotImplementedException();
        }
    }
    public class ThColumnInfoCommands
    {
        [CommandMethod("TIANHUACAD", "ThCic", CommandFlags.Modal)]
        public void ThColumnInfoCheckWindow()
        {
            //CheckPalette.Instance.Show();
            //DataPalette.Instance.Show();
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptPointResult ppr1 = ed.GetPoint("\n请选择要校对的柱子第一个范围点");
            if (ppr1.Status != PromptStatus.OK)
            {
                return;
            }
            PromptPointResult ppr2 = ed.GetCorner("\n请选择要校对的柱子第二个范围点", ppr1.Value);
            if (ppr2.Status != PromptStatus.OK)
            {
                return;
            }
            Point3d firstPt = ppr1.Value.TransformBy(ed.CurrentUserCoordinateSystem);
            Point3d secondPt = ppr2.Value.TransformBy(ed.CurrentUserCoordinateSystem);
            List<string> lockedLayerNames = ThColumnInfoUtils.UnlockedAllLayers();
            try
            {
                //获取本地柱子的位置和柱表信息
                IDataSource dataSource = new ExtractColumnPosition(firstPt, secondPt);
                dataSource.Extract();

                //Test
                ParameterSetInfo paraSetInfo = new ParameterSetInfo();
                paraSetInfo.AntiSeismicGrade = "抗震等级：二级";
                paraSetInfo.FloorCount = 3;
                paraSetInfo.ProtectLayerThickness = 30;

                ThNoCalculationValidate thNoCalValidate = new ThNoCalculationValidate(dataSource, paraSetInfo);
                thNoCalValidate.Validate();

                //获取数据库的信息
                string dbPath = @"C:\Users\liuguangsheng\AppData\Roaming\eSpace_Desktop\UserData\liuguangsheng\ReceiveFile\实例 - Send 1023\实例 - Send 1023\A3#楼 - 伪原位\计算模型\施工图\dtlmodel.ydb";
                IDatabaseDataSource dbDataSource = new ExtractYjkColumnInfo(dbPath);
                dbDataSource.Extract(4);

                //让用户指定柱子的位置
                ThDrawColumns thDrawColumns = new ThDrawColumns(dbDataSource.ColumnInfs);
                thDrawColumns.Draw();

                if (thDrawColumns.IsGoOn)
                {
                    //位置确定后，关联本地柱子
                    ThRelateColumn thRelateColumn = new ThRelateColumn(dataSource.ColumnInfs, thDrawColumns.ColumnRelateInfs);
                    thRelateColumn.Relate();
                }
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "");
            }
            finally
            {
                if (lockedLayerNames.Count > 0)
                {
                    ThColumnInfoUtils.LockedLayers(lockedLayerNames);
                }
            }
        }

        public static ImportCalculation columnCalulationInstance = null;
        [CommandMethod("TIANHUACAD", "ThCci", CommandFlags.Modal)]
        public void ThColumnCalculationImport()
        {
            try
            {
                CalculationInfo calculationInfo = new CalculationInfo()
                {
                    YjkPath = "",
                    SelectByFloor = true,
                    SelectByStandard = false,
                    Angle = 45,
                    ModelAppoint = true,
                    QuickAppoint = "1,2,3"
                };
                CalculationInfoVM calculationInfoVM = new CalculationInfoVM(calculationInfo);
                ImportCalculation importCalculation = new ImportCalculation(calculationInfoVM);
                columnCalulationInstance = importCalculation;
                importCalculation.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                importCalculation.ShowDialog();
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex);
            }
            finally
            {
                columnCalulationInstance = null;
            }
        }
    }
}
