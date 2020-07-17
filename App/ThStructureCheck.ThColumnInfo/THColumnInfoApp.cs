using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using acadApp = Autodesk.AutoCAD.ApplicationServices;
using ThColumnInfo.ViewModel;
using ThColumnInfo.View;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using ThColumnInfo.Service;

[assembly: CommandClass(typeof(ThColumnInfo.ThColumnInfoCommands))]
[assembly: ExtensionApplication(typeof(ThColumnInfo.ThColumnInfoApp))]
namespace ThColumnInfo
{
    public class ThColumnInfoApp : IExtensionApplication
    {
        public void Initialize()
        {
            DocumentCollection dc = acadApp.Application.DocumentManager;
            dc.DocumentDestroyed += Dc_DocumentDestroyed;
            dc.DocumentActivated += Dc_DocumentActivated;
        }

        private void Dc_DocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            if (e.Document == null)
            {
                return;
            }
            try
            {
                if(!ThColunmDocManager.IsExisted(e.Document.Name))
                {
                    CheckPalette._checkResult.tvCheckRes.Nodes.Clear();
                    if (DataPalette._dateResult != null)
                    {
                        DataPalette._dateResult.ClearDataGridView();
                    }
                }
                else
                {
                    CheckPalette._checkResult.CheckResVM.Reset(acadApp.Application.DocumentManager.MdiActiveDocument.Name);
                    if (DataPalette.ShowPaletteMark)
                    {
                        CheckPalette._checkResult.CheckResVM.ExportDetailData(CheckPalette._checkResult.CheckResVM.LastShowDetailNode);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "Dc_DocumentActivated");
            }
        }
        private void Dc_DocumentDestroyed(object sender, DocumentDestroyedEventArgs e)
        {
            try
            {
                if (CheckPalette._checkResult == null || CheckPalette._checkResult.tvCheckRes == null ||
                    CheckPalette._checkResult.tvCheckRes.Nodes.Count == 0)
                {
                    return;
                }
                CheckPalette._checkResult.tvCheckRes.Nodes.Clear();
                ThColunmDocManager.DeleteThStandardSignManager(e.FileName);
                CheckPalette._ps.Visible = false;
                DataPalette._ps.Visible = false;
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "Dc_DocumentDestroyed");
            }
        }
        public void Terminate()
        {
            DocumentCollection dc = acadApp.Application.DocumentManager;
            dc.DocumentDestroyed -= Dc_DocumentDestroyed;
            dc.DocumentActivated -= Dc_DocumentActivated;
        }
    }
    public class ThColumnInfoCommands
    {
        [CommandMethod("TIANHUACAD", "ThCPI", CommandFlags.Modal)]
        public void ThColumnParameterSet()
        {
            if(ParameterSetVM.isOpened)
            {
                return;
            }
            try
            {
                ParameterSetVM parameterSetVM = new ParameterSetVM();
                if(parameterSetVM.ParaSetInfo.FloorCount==0 && CheckPalette._checkResult.tvCheckRes.SelectedNode!=null)
                {
                    if(CheckPalette._checkResult.tvCheckRes.SelectedNode.Tag!=null &&
                        CheckPalette._checkResult.tvCheckRes.SelectedNode.Tag.GetType()==typeof(ThStandardSignManager))
                    {
                        ThStandardSignManager tssm = CheckPalette._checkResult.tvCheckRes.SelectedNode.Tag as ThStandardSignManager;
                        parameterSetVM.ParaSetInfo.FloorCount = tssm.StandardSigns.Count;
                    }
                }
                ParameterSet parameterSet = new ParameterSet(parameterSetVM);
                parameterSetVM.Owner = parameterSet;
                parameterSet.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                parameterSet.Topmost = true;
                parameterSet.Show();
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex);
                ParameterSetVM.isOpened = false;
            }
        }
        
        [CommandMethod("TIANHUACAD", "ThCRC", CommandFlags.Modal)]
        public void ThColumnInfoCrc()
        {
            try
            {
                ParameterSetVM parameterSetVM = new ParameterSetVM();
                if (parameterSetVM.ParaSetInfo.FloorCount == 0)
                {
                    var res = ThStandardSignManager.LoadData("", false);
                    parameterSetVM.ParaSetInfo.FloorCount = res.StandardSigns.Count;
                    parameterSetVM.SaveFloorCountToDatabase();
                }
                //创建柱校核提醒图层
                BaseFunction.CreateColumnLayer();
                //显示结果
                CheckPalette.Instance.Show();
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "ThColumnInfoCrc");
            }
        }
        [CommandMethod("TIANHUACAD", "ThTest", CommandFlags.Modal)]
        public void ThTest()
        {
            var doc = acadApp.Application.DocumentManager.MdiActiveDocument;
            var per = doc.Editor.GetEntity("\n选择柱外框线");
            if (per.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
            {
                using (Transaction trans = doc.TransactionManager.StartTransaction())
                {
                    Curve curve = trans.GetObject(per.ObjectId, OpenMode.ForRead) as Curve;
                    Autodesk.AutoCAD.EditorInput.PromptSelectionResult psr = doc.Editor.GetSelection();
                    List<DBText> dBTexts = new List<DBText>();
                    if (psr.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                    {
                        foreach (ObjectId objId in psr.Value.GetObjectIds())
                        {
                            if (trans.GetObject(objId, OpenMode.ForRead) is DBText dbText)
                            {
                                dBTexts.Add(dbText);
                            }
                        }
                    }
                    BuildInSituMarkInf buildInSituMarkInf = new BuildInSituMarkInf(curve, dBTexts);
                    buildInSituMarkInf.Build();
                    doc.Editor.WriteMessage("\n柱号: " + buildInSituMarkInf.Ctri.Code);
                    doc.Editor.WriteMessage("\n规格: " + buildInSituMarkInf.Ctri.Spec);
                    doc.Editor.WriteMessage("\n箍筋: " + buildInSituMarkInf.Ctri.HoopReinforcement);
                    doc.Editor.WriteMessage("\n箍筋类型号: " + buildInSituMarkInf.Ctri.HoopReinforcementTypeNumber);
                    doc.Editor.WriteMessage("\n全部纵筋: " + buildInSituMarkInf.Ctri.AllLongitudinalReinforcement);
                    doc.Editor.WriteMessage("\n角筋: " + buildInSituMarkInf.Ctri.AngularReinforcement);
                    doc.Editor.WriteMessage("\nB边: " + buildInSituMarkInf.Ctri.BEdgeSideMiddleReinforcement);
                    doc.Editor.WriteMessage("\nH边: " + buildInSituMarkInf.Ctri.HEdgeSideMiddleReinforcement);
                    trans.Commit();
                }
            }
        }
        [CommandMethod("TIANHUACAD", "ThLook", CommandFlags.Modal)]
        public void LookEmbededColumn()
        {
            PlantCalDataToDraw plantCal = new PlantCalDataToDraw();
            plantCal.GetEmbededColumnIds();
            if (plantCal.EmbededColumnIds.Count == 0)
            {
                MessageBox.Show("未能发现任何埋入的柱子实体，无法浏览埋入的数据!");
                return;
            }
            Document doc = acadApp.Application.DocumentManager.MdiActiveDocument;
            try
            {
                ThColumnInfoUtils.ShowObjIds(plantCal.EmbededColumnIds.ToArray(), true);
                bool domark = true;
                while(domark)
                {
                   var res= doc.Editor.GetEntity("\n选择埋入的柱子实体");
                    if(res.Status== Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                    {
                        var embededData = plantCal.GetExtensionDictionary(res.ObjectId);
                        doc.Editor.WriteMessage("\n---------------YJK数据---------------");
                        doc.Editor.WriteMessage("\nJtID: " + embededData.Item3.JtID);
                        doc.Editor.WriteMessage("\nFloorID: " + embededData.Item3.FloorID);
                        doc.Editor.WriteMessage("\nStdFlrID: " + embededData.Item3.StdFlrID);
                        doc.Editor.WriteMessage("\n剪跨比: "+ embededData.Item1.Jkb);
                        doc.Editor.WriteMessage("\n轴压比: " + embededData.Item1.AxialCompressionRatio);
                        doc.Editor.WriteMessage("\n轴压比限值: "+ embededData.Item1.AxialCompressionRatioLimited);
                        doc.Editor.WriteMessage("\n角筋直径限值: "+ embededData.Item1.ArDiaLimited);
                        doc.Editor.WriteMessage("\n抗震等级: " + embededData.Item1.AntiSeismicGrade);
                        doc.Editor.WriteMessage("\n保护层厚度: "+ embededData.Item1.ProtectThickness);
                        doc.Editor.WriteMessage("\n是否角柱: " + embededData.Item1.IsCorner);
                        doc.Editor.WriteMessage("\n结构类型: "+ embededData.Item1.StructureType);
                        doc.Editor.WriteMessage("\n配筋面积限值(X向限值): "+ embededData.Item1.DblXAsCal);
                        doc.Editor.WriteMessage("\n配筋面积限值(Y向限值): " + embededData.Item1.DblYAsCal);
                        doc.Editor.WriteMessage("\n是否底层: "+ embededData.Item1.IsGroundFloor);
                        doc.Editor.WriteMessage("\n设防烈度: "+ embededData.Item1.FortiCation);
                        doc.Editor.WriteMessage("\n体积配筋率限值: "+ embededData.Item1.VolumeReinforceLimitedValue);
                        doc.Editor.WriteMessage("\n配筋面积限值(DblStirrupAsCal): "+ embededData.Item1.DblStirrupAsCal);
                        doc.Editor.WriteMessage("\n配筋面积限值(DblStirrupAsCal0): " + embededData.Item1.DblStirrupAsCal0);
                        doc.Editor.WriteMessage("\n假定箍筋间距: "+ embededData.Item1.IntStirrupSpacingCal);

                        doc.Editor.WriteMessage("\n---------------用户自定义数据---------------");
                        doc.Editor.WriteMessage("\n抗震等级: " + embededData.Item2.AntiSeismicGrade);
                        doc.Editor.WriteMessage("\n混凝土强度: "+ embededData.Item2.ConcreteStrength);
                        doc.Editor.WriteMessage("\n保护层厚度: " + embededData.Item2.ProtectLayerThickness);
                        doc.Editor.WriteMessage("\n是否角柱: "+ embededData.Item2.CornerColumn);
                        doc.Editor.WriteMessage("\n箍筋全高度加密: " + embededData.Item2.HoopReinforceFullHeightEncryption);
                        doc.Editor.WriteMessage("\n箍筋放大倍数: " + embededData.Item2.HoopReinforcementEnlargeTimes);
                        doc.Editor.WriteMessage("\n纵筋放大倍数: " + embededData.Item2.LongitudinalReinforceEnlargeTimes);
                    }
                    else if (res.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.Cancel)
                    {
                        domark = false;
                    }
                }
            }
            finally
            {
                ThColumnInfoUtils.ShowObjIds(plantCal.EmbededColumnIds.ToArray(), false);
            }
        }
        [CommandMethod("TIANHUACAD", "ThEraseColumnFrame", CommandFlags.Modal)]
        public void EraseColumnFrameOrText()
        {
            try
            {
                Document doc = acadApp.Application.DocumentManager.MdiActiveDocument;
                TypedValue[] tvs = new TypedValue[]
                {
                new TypedValue((int)DxfCode.ExtendedDataRegAppName,ThColumnInfoUtils.thColumnFrameRegAppName),
                new TypedValue((int)DxfCode.Start,"LWPOLYLINE,Text"),
                };
                SelectionFilter sf = new SelectionFilter(tvs);
                PromptSelectionOptions options = new PromptSelectionOptions();
                options.MessageForAdding = "请删除【导入计算书】或【校核】产生的框线或文字";
                options.RejectObjectsOnLockedLayers = true;
                PromptSelectionResult psr = doc.Editor.GetSelection(sf);
                if (psr.Status == PromptStatus.OK)
                {
                    ThColumnInfoUtils.EraseObjIds(psr.Value.GetObjectIds());
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "EraseFrameIds");
            }
        }
    }
}
