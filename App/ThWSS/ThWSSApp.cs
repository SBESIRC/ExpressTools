using System;
using AcHelper;
using Linq2Acad;
using DotNetARX;
using ThWss.View;
using ThWSS.Beam;
using ThWSS.Model;
using ThWSS.Engine;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThStructure.BeamInfo.Command;
using ThStructure.BeamInfo.Business;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThWSS
{
    public class ThWSSApp : IExtensionApplication
    {
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
        }

        [CommandMethod("TIANHUACAD", "THCalOBB", CommandFlags.Modal)]
        public void ThDistinguishBeam()
        {
            ThSparyLayoutSet instance = new ThSparyLayoutSet();
            var result = instance.ShowDialog();
            if (result == false)
            {
                return;
            }

            var layoutModel = SetWindowValues(instance);
            Run(layoutModel);
        }

        [CommandMethod("TIANHUACAD", "-THCalOBB", CommandFlags.Modal)]
        public void ThDistinguishBeamCLI()
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                // 指定布置对象
                PromptKeywordOptions keywordOptions = new PromptKeywordOptions("\n请指定布置对象：")
                {
                    AllowNone = true
                };
                keywordOptions.Keywords.Add("FrameLine", "FrameLine", "来自框线(L)");
                keywordOptions.Keywords.Add("FireCompartment ", "FireCompartment", "防火分区(F)");
                keywordOptions.Keywords.Add("CustomRegion", "CustomRegion", "自定义区域(C)");
                keywordOptions.Keywords.Default = "FrameLine";
                PromptResult result = Active.Editor.GetKeywords(keywordOptions);
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var layoutModel = new SprayLayoutModel();
                if (result.StringResult == "FrameLine")
                {
                    layoutModel.sparyLayoutWay = LayoutWay.fire;
                }
                else if (result.StringResult == "CustomRegion")
                {
                    layoutModel.sparyLayoutWay = LayoutWay.customPart;
                }
                else if (result.StringResult == "FireCompartment")
                {
                    layoutModel.sparyLayoutWay = LayoutWay.frame;
                }
                Run(layoutModel);
            }
        }

        [CommandMethod("TIANHUACAD", "THGETBEAMINFO", CommandFlags.Modal)]
        public void THGETBEAMINFO()
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            using (ThBeamDbManager beamManager = new ThBeamDbManager(acdb.Database))
            {
                ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
                var beamCurves = ThBeamGeometryService.Instance.BeamCurves(beamManager);
                thDisBeamCommand.CalBeamStruc(beamCurves);
            }
        }

        [CommandMethod("TIANHUACAD", "THGETBEAMINFO2", CommandFlags.Modal)]
        public void THGETBEAMINFO2()
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                // 选择对象
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == "ARC,LINE,LWPOLYLINE" & o.Dxf((int)DxfCode.LayerName) == "S_BEAM");
                var entSelected = Active.Editor.GetSelection(options, filterlist);
                if (entSelected.Status != PromptStatus.OK)
                {
                    return;
                };

                // 执行操作
                DBObjectCollection dBObjects = new DBObjectCollection();
                foreach (ObjectId obj in entSelected.Value.GetObjectIds())
                {
                    var entity = acdb.Element<Entity>(obj);
                    dBObjects.Add(entity.GetTransformedCopy(Matrix3d.Identity));
                }

                ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
                thDisBeamCommand.CalBeamStruc(dBObjects);
            }
        }

        [CommandMethod("TIANHUACAD", "THMERGEBEAMCURVES", CommandFlags.Modal)]
        public void ThMergeBeamCurves()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", new string[]
                {
                    RXClass.GetClass(typeof(Arc)).DxfName,
                    RXClass.GetClass(typeof(Line)).DxfName,
                    RXClass.GetClass(typeof(Polyline)).DxfName,
                }));
                var result = Active.Editor.GetSelection(options, filterlist);
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var objs = new DBObjectCollection();
                foreach (var objId in result.Value.GetObjectIds())
                {
                    var obj = acadDatabase.Element<Entity>(objId, true);
                    objs.Add(obj.GetTransformedCopy(Matrix3d.Identity));
                }
                var results = ThBeamGeometryPreprocessor.MergeCurves(objs);
                if (results.Count == 0)
                {
                    return;
                }

                // 将合并后的图元添加到图纸中
                // 未参与合并的图元会被重新添加到图纸中
                foreach (Entity obj in results)
                {
                    acadDatabase.ModelSpace.Add(obj);
                }
                // 将原图元从图纸中删除
                foreach (var objId in result.Value.GetObjectIds())
                {
                    acadDatabase.Element<Entity>(objId, true).Erase();
                }
            }
        }

        /// <summary>
        /// 执行布置喷淋
        /// </summary>
        /// <returns></returns>
        private void Run(SprayLayoutModel layoutModel)
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                if (layoutModel.sparyLayoutWay == LayoutWay.fire)
                {
                    // 选择防火分区
                    PromptSelectionOptions options = new PromptSelectionOptions()
                    {
                        AllowDuplicates = false,
                        RejectObjectsOnLockedLayers = true,
                    };
                    var filterlist = OpFilter.Bulid(o =>
                        o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Polyline)).DxfName);
                    var entSelected = Active.Editor.GetSelection(options, filterlist);
                    if (entSelected.Status != PromptStatus.OK)
                    {
                        return;
                    }

                    // 执行操作
                    foreach (var obj in entSelected.Value.GetObjectIds())
                    {
                        var polygon = acdb.Element<Polyline>(obj);
                        ThSprayLayoutEngine.Instance.Layout(acdb.Database, polygon, layoutModel);
                    }
                }
                else if (layoutModel.sparyLayoutWay == LayoutWay.frame)
                {
                    // 选取房间框线
                    PromptSelectionOptions options = new PromptSelectionOptions()
                    {
                        AllowDuplicates = false,
                        RejectObjectsOnLockedLayers = true,
                    };
                    var filterlist = OpFilter.Bulid(o =>
                        o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Polyline)).DxfName);
                    var entSelected = Active.Editor.GetSelection(options, filterlist);
                    if (entSelected.Status != PromptStatus.OK)
                    {
                        return;
                    }

                    // 获取房间框线
                    var rooms = new List<Polyline>();
                    foreach (var obj in entSelected.Value.GetObjectIds())
                    {
                        var pline = acdb.Element<Polyline>(obj);
                        rooms.Add(pline.GetTransformedCopy(Matrix3d.Identity) as Polyline);
                    }

                    // 执行操作
                    ThSprayLayoutEngine.Instance.Layout(rooms, layoutModel);
                }
                else if (layoutModel.sparyLayoutWay == LayoutWay.customPart)
                {
                    // 选择自定义区域
                    Polyline pline = new Polyline() {
                        Closed = true
                    };
                    using (PointCollector pc = new PointCollector(PointCollector.Shape.Polygon))
                    {
                        try
                        {
                            pc.Collect();
                            pline.CreatePolyline(pc.CollectedPoints);
                        }
                        catch
                        {
                            return;
                        }
                    }

                    // 执行操作
                    var rooms = new List<Polyline>()
                    {
                        pline,
                    };
                    ThSprayLayoutEngine.Instance.Layout(rooms, layoutModel);
                }
            }
        }

        private SprayLayoutModel SetWindowValues(ThSparyLayoutSet thSpary)
        {
            SprayLayoutModel layoutModel = new SprayLayoutModel();
            if (thSpary.fire.IsChecked == true)
            {
                layoutModel.sparyLayoutWay = LayoutWay.fire;
            }
            else if (thSpary.frame.IsChecked == true)
            {
                layoutModel.sparyLayoutWay = LayoutWay.frame;
            }
            else if (thSpary.customPart.IsChecked == true)
            {
                layoutModel.sparyLayoutWay = LayoutWay.customPart;
            }

            layoutModel.sparySSpcing = Convert.ToDouble(thSpary.customControl.sparySSpcing.Text);
            layoutModel.sparyESpcing = Convert.ToDouble(thSpary.customControl.sparyESpcing.Text);
            layoutModel.otherSSpcing = Convert.ToDouble(thSpary.customControl.otherSSpcing.Text);
            layoutModel.otherESpcing = Convert.ToDouble(thSpary.customControl.otherESpcing.Text);

            return layoutModel;
        }
    }
}
